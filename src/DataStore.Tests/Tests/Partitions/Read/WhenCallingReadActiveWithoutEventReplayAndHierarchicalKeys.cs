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

    public class WhenCallingReadActiveWithoutEventReplayAndHierarchicalKeys
    {
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeIdKey>();
            Assert.Equal(2, results.Count());
        }
        
        
        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTenantIdKey>(null, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.Single(results);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTenantTimePeriodKey>(
                             null, options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Single(results);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadActiveWithoutEventReplayAndHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.WithoutEventReplay.ReadActive<AggregateWithTypeTimePeriodIdKey>();
            Assert.Equal(2, results.Count());
        }
    }
}