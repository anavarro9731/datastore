namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhere
    {
        private readonly Guid carId;

        private readonly IEnumerable<Car> result;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardWhere()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhere), new DataStoreOptions() { UseVersionHistory = true});

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
            this.result = this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.Id == this.carId).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public void ItShouldReturnTheItemsDeleted()
        {
            Assert.Equal(this.carId, this.result.Single().Id);
        }

        [Fact]
        public void ItShouldRemoveAllHistoryItems()
        {
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }
    }
}