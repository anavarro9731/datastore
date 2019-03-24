namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhere
    {
        private  Guid carId;

        private  IEnumerable<Car> result;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhere), new DataStoreOptions() { UseVersionHistory = true});

            this.carId = Guid.NewGuid();
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
            this.result = await this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == this.carId);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldReturnTheItemsDeleted()
        {
            await Setup();
            Assert.Equal(this.carId, this.result.Single().id);
        }

        [Fact]
        public async void ItShouldRemoveAllHistoryItems()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }
    }
}