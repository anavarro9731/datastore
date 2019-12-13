namespace DataStore.Tests.Tests.IDocumentRepository.Create
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

    public class WhenCallingCreateWithHistoryLoggingEnabled
    {
        private Guid newCarId;

        private ITestHarness testHarness;

        private Guid unitOfWorkId;

        [Fact]
        public async void ItShouldAddAHistoryIndexEntityToTheHistoryAggregate()
        {
            await Setup();
            var aggregateHistoryItemHeader = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single();

            Assert.Equal(1, aggregateHistoryItemHeader.VersionId);
            Assert.Equal(this.unitOfWorkId, aggregateHistoryItemHeader.UnitWorkId);
            Assert.Equal(typeof(Car).AssemblyQualifiedName, aggregateHistoryItemHeader.AssemblyQualifiedTypeName);
        }

        [Fact]
        public async void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var aggregateHistoryItem = this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistoryItem.id);
            Assert.True(aggregateHistoryItem.UnitOfWorkResponsibleForStateChange == this.unitOfWorkId);
            Assert.True(aggregateHistoryItem.AggregateVersion.id == this.newCarId);
        }

        [Fact]
        public async void ItShouldCreateAnAggregateHistoryRecord()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistory<Car>>));

            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistory.id);
            Assert.Equal(this.newCarId, aggregateHistory.AggregateId);
            Assert.Equal(1, aggregateHistory.Version);
        }

        [Fact]
        public async void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            await Setup();
            Assert.Equal(
                this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().Single().id,
                this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single().AggegateHistoryItemId);
        }

        private async Task Setup()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHistoryLoggingEnabled),
                DataStoreOptions.Create().WithVersionHistory(this.unitOfWorkId));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            //When
            await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}