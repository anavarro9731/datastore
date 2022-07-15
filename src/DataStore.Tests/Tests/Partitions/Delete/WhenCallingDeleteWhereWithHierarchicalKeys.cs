namespace DataStore.Tests.Tests.Partitions.Delete
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteWhereWithHierarchicalKeys
    {
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldDeleteIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();
            

            //when
            var result = await testHarness.DataStore.DeleteWhere<AggregateWithTypeIdKey>(x => x.id == newId);
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
                nameof(WhenCallingDeleteWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: true);

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
            var result = await testHarness.DataStore.DeleteWhere<AggregateWithTypeTenantIdKey>(x => x.id == newId, options => options.ProvidePartitionKeyValues(tenantId));
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
                nameof(WhenCallingDeleteWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var dayInterval = DayInterval.FromDateTime(DateTime.UtcNow);
            
            var agg = new AggregateWithTypeTimePeriodIdKey()
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();
            

            //when
            var result = await testHarness.DataStore.DeleteWhere<AggregateWithTypeTimePeriodIdKey>(x => x.id == newId, options => options.ProvidePartitionKeyValues(dayInterval));
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
                nameof(WhenCallingDeleteWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldDeleteIt),
                useHierarchicalPartitionKey: true);

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
            var result = await testHarness.DataStore.DeleteWhere<AggregateWithTypeTenantTimePeriodKey>(x => x.id == newId, options => options.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            var count = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantTimePeriodKey>(x => x.id == newId);
            Assert.Equal(0, count);
        }
    }
}