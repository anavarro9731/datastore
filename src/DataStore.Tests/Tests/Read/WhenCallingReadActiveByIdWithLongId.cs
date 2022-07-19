namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadActiveByIdWithLongId
    {
        private ITestHarness testHarness;

        private Guid id;

        [Fact]
        public async void ItShouldTransferTheOptionsCorrectlyForTenantTimePeriodKey()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdWithLongId), useHierarchicalPartitionKey: false);

            this.id = Guid.NewGuid();
            var agg = await this.testHarness.DataStore.Create(
                          new AggregateWithTypeTenantTimePeriodKey()
                          {
                              id = this.id, 
                              TestValue = 1,
                              TenantId = Guid.NewGuid(),
                              TimeStamp = DateTime.UtcNow
                          });
            
            await this.testHarness.DataStore.CommitChanges();
            var longId = agg.GetLongId();
            
            var result = this.testHarness.DataStore.ReadActiveById<AggregateWithTypeTenantTimePeriodKey>(longId);
            Assert.NotNull(result);
            
        }
        
        [Fact]
        public async void ItShouldTransferTheOptionsCorrectlyForTypeIdKey()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdWithLongId), useHierarchicalPartitionKey:false);

            this.id = Guid.NewGuid();
            var agg = await this.testHarness.DataStore.Create(
                          new AggregateWithTypeIdKey()
                          {
                              id = this.id, 
                              TestValue = 1
                          });
            
            await this.testHarness.DataStore.CommitChanges();
            var longId = agg.GetLongId();
            
            var result = this.testHarness.DataStore.ReadById<AggregateWithTypeIdKey>(longId);
            Assert.NotNull(result);
            
        }
        
                
        [Fact]
        public async void ItShouldTransferTheOptionsCorrectlyForSharedKey()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdWithLongId), useHierarchicalPartitionKey:false);

            this.id = Guid.NewGuid();
            var agg = await this.testHarness.DataStore.Create(
                          new AggregateWithSharedKey()
                          {
                              id = this.id, 
                              TestValue = 1
                          });
            
            await this.testHarness.DataStore.CommitChanges();
            var longId = agg.GetLongId();
            
             var result = await this.testHarness.DataStore.ReadById<AggregateWithSharedKey>(longId);
             Assert.NotNull(result);
            
            
        }

    }
}