namespace DataStore.Tests.Tests.Delete
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteSoftByIdUsingLongId
    {
        private Guid id;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldTransferTheOptionsCorrectly()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoftByIdUsingLongId));

            this.id = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var agg = await this.testHarness.DataStore.Create(
                          new AggregateWithTypeTenantTimePeriodKey
                          {
                              id = this.id, TestValue = 1, TenantId = tenantId, TimeStamp = DateTime.UtcNow
                          });

            await this.testHarness.DataStore.CommitChanges();
            var longId = agg.GetLongId();

            var count1 = await this.testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantTimePeriodKey>(
                             setOptions: options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromUtcNow()));
            Assert.Equal(1, count1);

            //When
            await this.testHarness.DataStore.DeleteById<AggregateWithTypeTenantTimePeriodKey>(longId);
            await this.testHarness.DataStore.CommitChanges();

            var count2 = await this.testHarness.DataStore.WithoutEventReplay.CountActive<AggregateWithTypeTenantTimePeriodKey>(
                             setOptions: options => options.ProvidePartitionKeyValues(tenantId, MonthInterval.FromUtcNow()));
            Assert.Equal(0, count2);
        }
    }
}