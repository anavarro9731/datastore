namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadByIdWithLongIdWithoutEventReplay
    {
        private ITestHarness testHarness;

        private Guid id;

        [Fact]
        public async void ItShouldTransferTheOptionsCorrectlyForTenantTimePeriodKey()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadByIdWithLongIdWithoutEventReplay));

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
            
            var result = this.testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantTimePeriodKey>(longId);
            Assert.NotNull(result);
            
        }
    }
}