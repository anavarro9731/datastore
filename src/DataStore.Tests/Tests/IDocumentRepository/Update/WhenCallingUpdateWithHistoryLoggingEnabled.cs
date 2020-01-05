namespace DataStore.Tests.Tests.IDocumentRepository.Update
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

    public class WhenCallingUpdateWithHistoryLoggingEnabled
    {
        private Guid carId;

        private ITestHarness testHarness;

        private Guid unitOfWorkId;

        async Task Setup()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWithHistoryLoggingEnabled),
                DataStoreOptions.Create().WithVersionHistory(this.unitOfWorkId));

            this.carId = Guid.NewGuid();
            var car = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };

            await this.testHarness.DataStore.Create(car);
            await this.testHarness.DataStore.CommitChanges();

            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);
            existingCarFromDb.Make = "Ford";

            //When
            await this.testHarness.DataStore.Update(existingCarFromDb);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldAddAHistoryIndexEntityToTheHistoryAggregate()
        {
            await Setup();
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory>().Single();

            Assert.Equal(2, aggregateHistory.AggregateVersions.Count);
            Assert.Equal(2, aggregateHistory.AggregateVersions.ToList()[1].VersionId);
            Assert.Equal(this.unitOfWorkId, aggregateHistory.AggregateVersions.ToList()[1].UnitWorkId);
            Assert.Equal(
                typeof(Car).AssemblyQualifiedName,
                aggregateHistory.AggregateVersions.ToList()[1].AssemblyQualifiedTypeName);
        }

        [Fact]
        public async void ItShouldCreateTheAggregateHistoryItemRecord()
        {
            await Setup();
            Assert.Equal(2, this.testHarness.DataStore.ExecutedOperations.Count(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var aggregateHistoryItems = this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().ToList();

            Assert.Equal(2, aggregateHistoryItems.Count());
            Assert.Equal("Volvo", aggregateHistoryItems.ToList()[0].AggregateVersion.Make);
            Assert.Equal("Ford", aggregateHistoryItems.ToList()[1].AggregateVersion.Make);
            Assert.NotEqual(Guid.Empty, aggregateHistoryItems.ToList()[1].id);
        }

        [Fact]
        public async void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            await Setup();
            Assert.Equal(
                this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().ToList()[1].id,
                this.testHarness.QueryDatabase<AggregateHistory>().Single().AggregateVersions.ToList()[1].AggegateHistoryItemId);
        }

        [Fact]
        public async void ItShouldUpdateTheAggregateHistoryRecord()
        {
            await Setup();
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistory.id);
            Assert.Equal(this.carId, aggregateHistory.AggregateId);
            Assert.Equal(2, aggregateHistory.Version);
        }
    }
}