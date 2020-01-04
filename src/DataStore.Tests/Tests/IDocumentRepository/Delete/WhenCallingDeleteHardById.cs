namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardById
    {
        private  Guid carId;

        private  Car result;

        private  ITestHarness testHarness;

         async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardById), DataStoreOptions.Create().WithVersionHistory(null));

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
            this.result = await this.testHarness.DataStore.DeleteHardById<Car>(this.carId);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldFlushTheSessionCache()
        {
            await Setup();
            Assert.Empty(this.testHarness.DataStore.QueuedOperations);
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car> && e.MethodCalled == nameof(DataStore.DeleteHardById)));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public async void ItShouldReturnTheItemDeleted()
        {
            await Setup();
            Assert.Equal(this.carId, this.result.id);
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