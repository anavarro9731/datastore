namespace DataStore.Tests.Tests.Partitions.Delete
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteByIdWithSyntheticKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteByIdWithSyntheticKeys) + nameof(AndAttributeWithTypeIdKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithSharedKey()
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();
            

            //when
            var result = await testHarness.DataStore.DeleteById<AggregateWithSharedKey>(newId);
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithSharedKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteByIdWithSyntheticKeys) + nameof(AndAttributeWithTypeIdKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();
            

            //when
            var result = await testHarness.DataStore.DeleteById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeIdKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteByIdWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldDeleteIt),
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
            var result = await testHarness.DataStore.DeleteById<AggregateWithTypeTenantIdKey>(newId, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantIdKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteByIdWithSyntheticKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldDeleteIt),
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
            var result = await testHarness.DataStore.DeleteById<AggregateWithTypeTimePeriodIdKey>(newId, options => options.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTimePeriodIdKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteByIdWithSyntheticKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            
            var monthInterval = MonthInterval.FromDateTime(DateTime.UtcNow);

            var agg = new AggregateWithTypeTenantTimePeriodKey()
            {
                id = newId,
                TenantId = tenantId,
                TimeStamp = DateTime.UtcNow
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();


            //when
            var result = await testHarness.DataStore.DeleteById<AggregateWithTypeTenantTimePeriodKey>(newId, options => options.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantTimePeriodKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }
    }
}