namespace DataStore.Tests.Tests.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdWithFullVersionHistoryEnabled
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistoryBeforeDelete;

        private List<AggregateHistoryItem<Car>> versionHistoryFullRecordBeforeDelete;

        [Fact]
        public async Task ItShouldCreateAFullVersionHistoryRecord()
        {
            await Setup();
            Assert.Single(this.versionHistoryFullRecordBeforeDelete);
        }

        [Fact]
        public async Task ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistoryBeforeDelete);
        }

        [Fact]
        public async void ItShouldDeleteAllTheHistory()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>());
        }

        private async Task Setup()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteHardByIdWithFullVersionHistoryEnabled),
                DataStoreOptions.Create().EnableFullVersionHistory());

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();

            this.versionHistoryBeforeDelete =
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().VersionHistory;

            this.versionHistoryFullRecordBeforeDelete = this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>();

            //When
            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.carId, o => o.Permanently());
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}