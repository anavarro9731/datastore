namespace DataStore.Tests.Tests.Partitions.Read
{
    using System;
    using System.Linq;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithSyntheticKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldRetrieveTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithSyntheticKeys) + nameof(AndAttributeWithSharedKeyItShouldRetrieveTheCorrectCount),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithSharedKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithSharedKey());
            await testHarness.DataStore.CommitChanges();

            //when
            var results = await testHarness.DataStore.Read<AggregateWithSharedKey>();
            Assert.Equal(2, results.Count());

            results = await testHarness.DataStore.Read<AggregateWithSharedKey>(x => x.id == newId);
            Assert.Single(results);
        }

        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithSyntheticKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());
            await testHarness.DataStore.Create(
                new AggregateWithTypeTenantIdKey
                {
                    TenantId = Guid.NewGuid()
                });
            await testHarness.DataStore.CommitChanges();

            //when
            var results = await testHarness.DataStore.Read<AggregateWithTypeIdKey>();
            Assert.Equal(2, results.Count());

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeIdKey>(null, options => options.ProvidePartitionKeyValues(Guid.NewGuid())));
            //throw when giving options and reading by type/id

            results = await testHarness.DataStore.Read<AggregateWithTypeIdKey>(x => x.id == newId);
            Assert.Single(results);

            results = await testHarness.DataStore.Read<AggregateWithTypeIdKey>(x => x.id == Guid.NewGuid());
            Assert.Empty(results);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldNotRequireFullKey() //because you dont know last part which is ID
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldNotRequireFullKey),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var tenantId = Guid.NewGuid();
            await testHarness.DataStore.Create(
                new AggregateWithTypeTenantIdKey
                {
                    TenantId = tenantId, Active = false
                });
            await testHarness.DataStore.Create(
                new AggregateWithTypeTenantIdKey
                {
                    TenantId = tenantId
                });
            await testHarness.DataStore.CommitChanges();

            //when

            var results = await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(); /* this is a fanout query */
            Assert.Equal(2, results.Count());

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(MonthInterval.FromDateTime(DateTime.UtcNow)))); //throw for irrelevant option

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(Guid.NewGuid()))); //throw for useless options
        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldUseFullKeyIfWeHaveItOrFanOut()
        {
            //requires full key or nothing because its possible to obtain it and there is no fallback in synth mode

            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldUseFullKeyIfWeHaveItOrFanOut),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var tenantId = Guid.NewGuid();
            await testHarness.DataStore.Create(
                new AggregateWithTypeTenantTimePeriodKey
                {
                    TenantId = tenantId, TimeStamp = DateTime.UtcNow
                });
            var thirtyDaysAgo = DateTime.UtcNow.Subtract(new TimeSpan(30, 0, 0, 0));
            await testHarness.DataStore.Create(
                new AggregateWithTypeTenantTimePeriodKey
                {
                    TenantId = tenantId, TimeStamp = thirtyDaysAgo
                });
            await testHarness.DataStore.CommitChanges();

            //when

            var results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                              null,
                              options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(thirtyDaysAgo)));
            Assert.Single(results);

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(tenantId))); //without full key throw

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(HourInterval.FromDateTime(DateTime.UtcNow))));

            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                          null,
                          options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UnixEpoch)));
            Assert.Empty(results); //with wrong key  fails to return anything

            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>();
            Assert.Equal(2, results.Count()); //without any key just fallback 

            var xCircuitException = await Assert.ThrowsAsync<CircuitException>(
                                        async () => await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                                                        null,
                                                        options => options.ProvidePartitionKeyValues(tenantId, MinuteInterval.FromDateTime(thirtyDaysAgo))));
            Assert.Equal(PartitionKeyHelpers.ErrorCodes.UsedIncorrectTimeInterval.ToString(), xCircuitException.Error.Key);
        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldNotRequireTheFullKey()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithSyntheticKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldNotRequireTheFullKey),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            await testHarness.DataStore.Create(new AggregateWithTypeTimePeriodIdKey());
            await testHarness.DataStore.Create(
                new AggregateWithTypeTimePeriodIdKey
                {
                    TestValue = 2
                });
            await testHarness.DataStore.CommitChanges();

            //when

            var results = await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(); /* this is a fanout query */
            Assert.Equal(2, results.Count());

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(DayInterval.FromDateTime(DateTime.UtcNow)))); //throw for useless option

            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(
                                null,
                                options => options.ProvidePartitionKeyValues(Guid.NewGuid()))); //throw when sending wrong type of option
        }
    }
}