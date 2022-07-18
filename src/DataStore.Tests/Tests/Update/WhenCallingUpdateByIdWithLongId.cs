namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdateByIdWithLongId
    {
        private Guid id;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldTransferTheOptionsCorrectly()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateByIdWithLongId));

            this.id = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var agg = await this.testHarness.DataStore.Create(
                          new AggregateWithTypeTenantTimePeriodKey
                          {
                              id = this.id, TestValue = 1, TenantId = tenantId, TimeStamp = DateTime.UtcNow
                          },
                          options => options.CreateReadonly());

            await this.testHarness.DataStore.CommitChanges();
            var longId = agg.GetLongId();

            var count1 = (await this.testHarness.DataStore.ReadActive<AggregateWithTypeTenantTimePeriodKey>(
                              x => x.id == this.id,
                              options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromUtcNow()))).Count();
            Assert.Equal(1, count1);

            //When
            await this.testHarness.DataStore.UpdateById<AggregateWithTypeTenantTimePeriodKey>(
                longId,
                key => key.TestValue = 2,
                options => options.OverwriteReadonly());
            await this.testHarness.DataStore.CommitChanges();

            var count2 = (await this.testHarness.DataStore.ReadActive<AggregateWithTypeTenantTimePeriodKey>(
                              x => x.id == this.id && x.TestValue == 2,
                              options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromUtcNow()))).Count();
            Assert.Equal(1, count2);
        }
    }
}