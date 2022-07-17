namespace DataStore.Tests.Tests.Partitions.Create
{
    #region

    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingCreateWithHierarchicalKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldThrowError()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithSharedKeyItShouldThrowError),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithSharedKey
            {
                id = newId
            };

            //when
            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.Create(agg));
            
            await AndShouldNotCreateDuplicates();
            async Task AndShouldNotCreateDuplicates() => await Assert.ThrowsAsync<CircuitException>(async () =>  await testHarness.DataStore.Create(agg));

        }

        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
                
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result);
            Assert.NotNull(result.PartitionKey);
            Assert.Equal(
                new HierarchicalPartitionKey
                {
                    Key1 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.Type}{typeof(AggregateWithTypeIdKey).Name}", 
                    Key2 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.IdOptional}{newId}",
                    Key3 = "_na"
                },
                result.PartitionKeys);
            
            await AndShouldNotCreateDuplicates();
            async Task AndShouldNotCreateDuplicates() => await Assert.ThrowsAsync<CircuitException>(async () =>  await testHarness.DataStore.Create(agg));
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            var agg = new AggregateWithTypeTenantIdKey
            {
                id = newId, TenantId = tenantId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantIdKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            Assert.NotNull(result.PartitionKey);
            Assert.Equal(
                new HierarchicalPartitionKey
                {
                    Key1 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.Type}{typeof(AggregateWithTypeTenantIdKey).Name}", 
                    Key2 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.TenantId}{tenantId}", 
                    Key3 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.IdOptional}{newId}"
                },
                result.PartitionKeys);
            
            await AndShouldNotCreateDuplicates();
            async Task AndShouldNotCreateDuplicates() => await Assert.ThrowsAsync<CircuitException>(async () =>  await testHarness.DataStore.Create(agg));

        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var monthInterval = MonthInterval.FromDateTime(DateTime.UtcNow);
            var tenantId = Guid.NewGuid();

            var agg = new AggregateWithTypeTenantTimePeriodKey
            {
                id = newId, TenantId = tenantId, TimeStamp = DateTime.UtcNow
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            Assert.NotNull(result.PartitionKey);
            Assert.Equal(
                new HierarchicalPartitionKey
                {
                    Key1 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.Type}{typeof(AggregateWithTypeTenantTimePeriodKey).Name}", 
                    Key2 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.TenantId}{tenantId}", 
                    Key3 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.TimePeriod}{monthInterval}"
                },
                result.PartitionKeys);
            
            await AndShouldNotCreateDuplicates();
            async Task AndShouldNotCreateDuplicates() => await Assert.ThrowsAsync<CircuitException>(async () =>  await testHarness.DataStore.Create(agg));

        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var dayInterval = DayInterval.FromDateTime(DateTime.UtcNow);

            var agg = new AggregateWithTypeTimePeriodIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTimePeriodIdKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result);
            Assert.NotNull(result.PartitionKey);
            Assert.Equal(
                new HierarchicalPartitionKey
                {
                    Key1 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.Type}{typeof(AggregateWithTypeTimePeriodIdKey).Name}", 
                    Key2 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.TimePeriod}{dayInterval}", 
                    Key3 = $"{PartitionKeyHelpers.PartitionKeyPrefixes.IdOptional }{newId}"
                },
                result.PartitionKeys);
            
            await AndShouldNotCreateDuplicates();
            async Task AndShouldNotCreateDuplicates() => await Assert.ThrowsAsync<CircuitException>(async () =>  await testHarness.DataStore.Create(agg));

        }
    }
}