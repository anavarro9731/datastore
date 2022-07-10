namespace DataStore.Tests.Tests.Partitions.Create
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithSyntheticKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithSyntheticKeys) + nameof(AndAttributeWithSharedKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: false);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithSharedKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithSharedKey>(newId);
            Assert.NotNull(result);
            Assert.Null(result.PartitionKeys);
            Assert.Equal("shared", result.PartitionKey);
        }
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithSyntheticKeys) + nameof(AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: false);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result);
            Assert.Null(result.PartitionKeys);
            Assert.Equal($"TP:{typeof(AggregateWithTypeIdKey).FullName}_ID:{newId}", result.PartitionKey);
        }
        
        
        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: false);
            
            var newId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeTenantIdKey()
            {
                id = newId,
                TenantId = tenantId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantIdKey>(newId, side => side.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKeys);
            Assert.Equal($"TP:{typeof(AggregateWithTypeTenantIdKey).FullName}_TN:{tenantId}_ID:{newId}", result.PartitionKey);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: false);
            
            var newId = Guid.NewGuid();
            var monthInterval = MonthInterval.FromDateTime(DateTime.UtcNow);
            var tenantId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeTenantTimePeriodKey()
            {
                id = newId,
                TenantId = tenantId,
                TimeStamp = DateTime.UtcNow
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantTimePeriodKey>(newId, side => side.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKeys);
            Assert.Equal($"TP:{typeof(AggregateWithTypeTenantTimePeriodKey).FullName}_TN:{tenantId}_TM:{monthInterval}", result.PartitionKey);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithSyntheticKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: false);
            
            var newId = Guid.NewGuid();
            var dayInterval = DayInterval.FromDateTime(DateTime.UtcNow);
            
            var agg = new AggregateWithTypeTimePeriodIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTimePeriodIdKey>(newId, side => side.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKeys);
            Assert.Equal($"TP:{typeof(AggregateWithTypeTimePeriodIdKey).FullName}_TM:{dayInterval}_ID:{newId}", result.PartitionKey);
        }
    }
}