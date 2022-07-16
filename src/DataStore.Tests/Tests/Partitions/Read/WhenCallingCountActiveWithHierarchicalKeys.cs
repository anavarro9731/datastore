namespace DataStore.Tests.Tests.Partitions.Read
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingCountActiveWithHierarchicalKeys
    {
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCountActiveWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount),
                useHierarchicalPartitionKey: true);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());
            await testHarness.DataStore.Create(new AggregateWithTypeTenantIdKey()
            {
                TenantId = Guid.NewGuid()
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeIdKey>();
            Assert.Equal(2, result);
        }
        
        
        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCountActiveWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount),
                useHierarchicalPartitionKey: true);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var tenantId = Guid.NewGuid();
            await testHarness.DataStore.Create(new AggregateWithTypeTenantIdKey()
            {
                TenantId = tenantId
            });
            await testHarness.DataStore.Create(new AggregateWithTypeTenantIdKey()
            {
                TenantId = Guid.NewGuid()
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantIdKey>(null, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.Equal(1, result);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCountActiveWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount),
                useHierarchicalPartitionKey: true);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var tenantId = Guid.NewGuid();
            await testHarness.DataStore.Create(new AggregateWithTypeTenantTimePeriodKey()
            {
                TenantId = tenantId,
                TimeStamp = DateTime.UtcNow
            });
            await testHarness.DataStore.Create(new AggregateWithTypeTenantTimePeriodKey()
            {
                TenantId = tenantId,
                TimeStamp = DateTime.UtcNow.Subtract(new TimeSpan(1000,0,0,0))
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantTimePeriodKey>(
                             null, options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Equal(1, result);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCountActiveWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount),
                useHierarchicalPartitionKey: true);
            
            var newId = Guid.NewGuid();
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = newId
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());
            
            await testHarness.DataStore.Create(new AggregateWithTypeTimePeriodIdKey()
            {
            });
            await testHarness.DataStore.Create(new AggregateWithTypeTimePeriodIdKey()
            {
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTimePeriodIdKey>(setOptions:o =>o.AcceptCrossPartitionQueryCost());
            Assert.Equal(2, result);
        }
    }
}