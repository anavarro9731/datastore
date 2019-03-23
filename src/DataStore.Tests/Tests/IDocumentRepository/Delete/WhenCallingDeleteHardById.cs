namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardById
    {
        private readonly Guid carId;

        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardById()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardById),
                new DataStoreOptions() { UseVersionHistory = true });

            this.carId = Guid.NewGuid();
            this.testHarness.DataStore.Create(
                new Car
                {
                    Id = this.carId,
                    Make = "Volvo"
                }).Wait();

            this.testHarness.DataStore.CommitChanges().Wait();
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());


            //When
            this.result = this.testHarness.DataStore.DeleteHardById<Car>(this.carId).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldFlushTheSessionCache()
        {
            Assert.Empty(this.testHarness.DataStore.QueuedOperations);
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public void ItShouldReturnTheItemDeleted()
        {
            Assert.Equal(this.carId, this.result.Id);
        }

        [Fact]
        public void ItShouldRemoveAllHistoryItems()
        {
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }
    }
}