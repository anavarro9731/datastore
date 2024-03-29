namespace DataStore.Tests.Tests.Partitions.Read
{
    #region

    using System;
    using System.Linq;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadWithHierarchicalKeys
    {
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.Read<AggregateWithTypeIdKey>();
            Assert.Equal(2, results.Count());
            
            results = await testHarness.DataStore.Read<AggregateWithTypeIdKey>(x => x.id == newId);
            Assert.Single(results);
        }
        
        
        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(null, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.Single(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(null, options => options.ProvidePartitionKeyValues(Guid.NewGuid()));
            Assert.Empty(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantIdKey>(setOptions:o =>o.AcceptCrossPartitionQueryCost());
            Assert.Equal(2, results.Count());
        }
        
        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                             null, options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Single(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                              null, options => options.ProvidePartitionKeyValues(Guid.NewGuid(), MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Empty(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                              null, options =>
                                  {
                                  options.AcceptCrossPartitionQueryCost();
                                  options.ProvidePartitionKeyValues(tenantId);
                                  });
            Assert.Equal(2, results.Count());
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                          null, options =>
                              {
                              options.AcceptCrossPartitionQueryCost();
                              options.ProvidePartitionKeyValues(Guid.NewGuid());
                              });
            Assert.Empty(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                              null, options =>
                                  {
                                  options.AcceptCrossPartitionQueryCost();
                                  options.ProvidePartitionKeyValues(MonthInterval.FromDateTime(DateTime.UtcNow));
                                  });
            Assert.Equal(2, results.Count()); //since it's last in the hierarchy and we haven't provided the middle tier
                                              //it's ignored for filtering even though its provided and should fall back to just type
                                              
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(setOptions:options =>options.AcceptCrossPartitionQueryCost());
            Assert.Equal(2, results.Count());
            
            var xCircuitException = await Assert.ThrowsAsync<CircuitException>( async () =>await testHarness.DataStore.Read<AggregateWithTypeTenantTimePeriodKey>(
                null, options => options.ProvidePartitionKeyValues(Guid.NewGuid(), MinuteInterval.FromDateTime(DateTime.UtcNow))));
            Assert.Equal(PartitionKeyHelpers.ErrorCodes.UsedIncorrectTimeInterval.ToString(), xCircuitException.Error.Key);
        }
        
        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldReturnTheCorrectCount),
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
            var results = await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(
                              null, options => options.ProvidePartitionKeyValues(DayInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Equal(2, results.Count());
            
            var xCircuitException = await Assert.ThrowsAsync<CircuitException>( async () => await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(
                null, options => options.ProvidePartitionKeyValues(MonthInterval.FromDateTime(DateTime.UtcNow))));
            Assert.Equal(PartitionKeyHelpers.ErrorCodes.UsedIncorrectTimeInterval.ToString(), xCircuitException.Error.Key);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(
                              null, options => options.ProvidePartitionKeyValues(DayInterval.FromDateTime(DateTime.UnixEpoch)));
            Assert.Empty(results);
            
            results = await testHarness.DataStore.Read<AggregateWithTypeTimePeriodIdKey>(setOptions:options =>options.AcceptCrossPartitionQueryCost());
            Assert.Equal(2, results.Count());
        }
    }
}