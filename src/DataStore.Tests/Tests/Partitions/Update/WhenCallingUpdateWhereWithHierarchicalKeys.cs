namespace DataStore.Tests.Tests.Partitions.Update
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdateWhereWithHierarchicalKeys
    {
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldUpdateIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldUpdateIt),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();
            

            //when
            var result = await testHarness.DataStore.UpdateWhere<AggregateWithTypeIdKey>(x => x.id == newId, a => a.TestValue = 1);
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            //Then
            var result2 = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result2);
            Assert.Equal(1, result2.TestValue);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldUpdateIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldUpdateIt),
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
            var result = await testHarness.DataStore.UpdateWhere<AggregateWithTypeTenantIdKey>(x => x.id == newId, a => a.TestValue = 1, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            //Then
            var result2 = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantIdKey>(newId, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result2);
            Assert.Equal(1, result2.TestValue);
        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldUpdateIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldUpdateIt),
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
            var result = await testHarness.DataStore.UpdateWhere<AggregateWithTypeTimePeriodIdKey>(x => x.id == newId, a => a.TestValue = 1, options => options.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            //Then
            var result2 = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTimePeriodIdKey>(newId, options => options.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result2);
            Assert.Equal(1, result2.TestValue);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldUpdateIt()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWhereWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldUpdateIt),
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
            var result = await testHarness.DataStore.UpdateWhere<AggregateWithTypeTenantTimePeriodKey>(x => x.id == newId, a => a.TestValue = 1, options => options.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            
            await testHarness.DataStore.CommitChanges();
            
            //Then
            var result2 = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantTimePeriodKey>(newId, options => options.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result2);
            Assert.Equal(1, result2.TestValue);
        }
    }
}