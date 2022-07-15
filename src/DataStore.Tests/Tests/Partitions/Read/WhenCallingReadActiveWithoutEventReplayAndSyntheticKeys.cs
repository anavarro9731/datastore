namespace DataStore.Tests.Tests.Partitions.Read
{
    #region

    using System;
    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldRetrieveTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys) + nameof(AndAttributeWithSharedKeyItShouldRetrieveTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithSharedKey>();
            Assert.Equal(2, results.Count());
        }

        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeIdKey>();
            Assert.Equal(2, results.Count());
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTenantIdKey>();
            Assert.Single(results);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTenantTimePeriodKey>(
                             null,
                             options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(thirtyDaysAgo)));
            Assert.Single(results);
        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndSyntheticKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTimePeriodIdKey>(x => x.TestValue == 2);
            Assert.Single(results);
        }
    }
}