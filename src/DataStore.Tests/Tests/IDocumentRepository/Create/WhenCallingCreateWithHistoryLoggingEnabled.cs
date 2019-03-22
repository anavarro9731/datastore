namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithHistoryLoggingEnabled
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        private readonly Guid unitOfWorkId;

        public WhenCallingCreateWithHistoryLoggingEnabled()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHistoryLoggingEnabled),
                new DataStoreOptions
                {
                    UnitOfWorkId = this.unitOfWorkId,
                    UseVersionHistory = true
                });

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            //When
            this.testHarness.DataStore.Create(newCar).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldAddAHistoryIndexEntityToTheHistoryAggregate()
        {
            var aggregateHistoryItemHeader = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single();

            Assert.Equal(1, aggregateHistoryItemHeader.VersionId);
            Assert.Equal(this.unitOfWorkId, aggregateHistoryItemHeader.UnitWorkId);
            Assert.Equal(typeof(Car).AssemblyQualifiedName, aggregateHistoryItemHeader.AssemblyQualifiedTypeName);
        }

        [Fact]
        public void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var aggregateHistoryItem = this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistoryItem.id);
            Assert.True(aggregateHistoryItem.UnitOfWorkResponsibleForStateChange == this.unitOfWorkId);
            Assert.True(aggregateHistoryItem.AggregateVersion.id == this.newCarId);
        }

        [Fact]
        public void ItShouldCreateAnAggregateHistoryRecord()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistory<Car>>));

            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistory.id);
            Assert.Equal(this.newCarId, aggregateHistory.AggregateId);
            Assert.Equal(1, aggregateHistory.Version);
        }

        [Fact]
        public void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            Assert.Equal(
                this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().Single().id,
                this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single().AggegateHistoryItemId);
        }
    }
}