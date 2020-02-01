namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdWithFullVersionHistoryEnabled
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        private List<AggregateHistoryItem<Car>> versionHistoryFullRecordBeforeDelete;

        private List<Aggregate.AggregateVersionInfo> versionHistoryBeforeDelete;

        [Fact]
        public async void ItShouldDeleteAllTheHistory()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }

        [Fact]
        public async Task ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistoryBeforeDelete);
        }

        [Fact]
        public async Task ItShouldCreateAFullVersionHistoryRecord()
        {
            await Setup();
            Assert.Single(this.versionHistoryFullRecordBeforeDelete);
        }


        private async Task Setup()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.testHarness =
                TestHarness.Create(nameof(WhenCallingDeleteHardByIdWithFullVersionHistoryEnabled), 
                    DataStoreOptions.Create().EnableFullVersionHistory());

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();

            this.versionHistoryBeforeDelete = this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().VersionHistory;

            this.versionHistoryFullRecordBeforeDelete = this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>();

            //When
            this.result = await this.testHarness.DataStore.DeleteHardById<Car>(this.carId);
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}