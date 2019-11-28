namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdWithVersionHistoryEnabled
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        private Guid unitOfWorkId;

         async  Task Setup()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteHardByIdWithVersionHistoryEnabled),
                DataStoreOptions.Create().WithVersionHistory(this.unitOfWorkId));

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());

            //When
            this.result = await this.testHarness.DataStore.DeleteHardById<Car>(this.carId);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldDeleteAllTheHistory()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }
        
    }
}