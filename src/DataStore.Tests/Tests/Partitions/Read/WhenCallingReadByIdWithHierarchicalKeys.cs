namespace DataStore.Tests.Tests.Partitions.Read
{
    using System;
    using System.Linq;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadByIdWithHierarchicalKeys
    {
        
        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldReturnTheRecord()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadByIdWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldReturnTheRecord),
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
            var result = await testHarness.DataStore.ReadById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result);
            
            result = await testHarness.DataStore.ReadById<AggregateWithTypeIdKey>(Guid.NewGuid());
            Assert.Null(result);
        }
        
        
        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldReturnTheRecordOnlyWithFullKey()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadByIdWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldReturnTheRecordOnlyWithFullKey),
                useHierarchicalPartitionKey: true);
            
            
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = Guid.NewGuid()
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var newId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            await testHarness.DataStore.Create(new AggregateWithTypeTenantIdKey()
            {
                id = newId,
                TenantId = tenantId
            });
            await testHarness.DataStore.Create(new AggregateWithTypeTenantIdKey()
            {
                TenantId = Guid.NewGuid()
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.ReadById<AggregateWithTypeTenantIdKey>(newId, options => options.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            
            result = await testHarness.DataStore.ReadById<AggregateWithTypeTenantIdKey>(Guid.NewGuid(), options => options.ProvidePartitionKeyValues(tenantId));
            Assert.Null(result);

            await Assert.ThrowsAsync<CircuitException>(async  ()=> await testHarness.DataStore.ReadById<AggregateWithTypeTenantIdKey>(newId));
            
            await Assert.ThrowsAsync<CircuitException>(async  ()=> await testHarness.DataStore.ReadById<AggregateWithTypeTenantIdKey>(newId, options => options.ProvidePartitionKeyValues(timeInterval:DayInterval.FromDateTime(DateTime.UtcNow))));
        }
        
        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheRecordOnlyWithTheFullKey()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadByIdWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldReturnTheRecordOnlyWithTheFullKey),
                useHierarchicalPartitionKey: true);
            
            
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = Guid.NewGuid()
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());

            var tenantId = Guid.NewGuid();
            var newId = Guid.NewGuid();
            await testHarness.DataStore.Create(new AggregateWithTypeTenantTimePeriodKey()
            {
                id = newId,
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
            var result = await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId, options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.NotNull(result);
            
            result = await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId, options => options.ProvidePartitionKeyValues(Guid.NewGuid(), MonthInterval.FromDateTime(DateTime.UtcNow)));
            Assert.Null(result);
            
            result = await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId, options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromDateTime(DateTime.UnixEpoch)));
            Assert.Null(result);
            
            await Assert.ThrowsAsync<CircuitException>(async () => 
                await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId, options => options.ProvidePartitionKeyValues(MonthInterval.FromDateTime(DateTime.UtcNow))));
            
            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                                                                       newId, options => options.ProvidePartitionKeyValues(Guid.NewGuid())));

            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                                                                       newId));

        }
        
        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldReturnTheRecordOnlyWithFullKey()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingReadByIdWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldReturnTheRecordOnlyWithFullKey),
                useHierarchicalPartitionKey: true);
            
            
            
            var agg = new AggregateWithTypeIdKey()
            {
                id = Guid.NewGuid()
            };
            
            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.Create(new AggregateWithTypeIdKey());
            
            var newId = Guid.NewGuid();
            await testHarness.DataStore.Create(new AggregateWithTypeTimePeriodIdKey()
            {
                id = newId
            });
            await testHarness.DataStore.Create(new AggregateWithTypeTimePeriodIdKey()
            {
            });
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.ReadById<AggregateWithTypeTimePeriodIdKey>(newId, options => options.ProvidePartitionKeyValues(DayInterval.FromDateTime(DateTime.UtcNow)));
            Assert.NotNull(result);
            
            result = await testHarness.DataStore.ReadById<AggregateWithTypeTimePeriodIdKey>(newId, options => options.ProvidePartitionKeyValues(DayInterval.FromDateTime(DateTime.UnixEpoch)));
            Assert.Null(result);
            
            await Assert.ThrowsAsync<CircuitException>(async  ()=> await testHarness.DataStore.ReadById<AggregateWithTypeTimePeriodIdKey>(newId));
            
            await Assert.ThrowsAsync<CircuitException>(async  ()=> await testHarness.DataStore.ReadById<AggregateWithTypeTimePeriodIdKey>(newId, options => options.ProvidePartitionKeyValues(tenantId:Guid.NewGuid())));
        }
    }
}