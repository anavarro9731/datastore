namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWithHistoryLoggingEnabled
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        private readonly Guid unitOfWorkId;

        public WhenCallingUpdateWithHistoryLoggingEnabled()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWithHistoryLoggingEnabled),
                new DataStoreOptions
                {
                    UnitOfWorkId = this.unitOfWorkId,
                    UseVersionHistory = true
                });

            this.carId = Guid.NewGuid();
            var car = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };

            this.testHarness.DataStore.Create(car).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();

            var existingCarFromDb = this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result;
            existingCarFromDb.Make = "Ford";

            //When
            this.testHarness.DataStore.Update(existingCarFromDb).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldAddAHistoryIndexEntityToTheHistoryAggregate()
        {
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

            Assert.Equal(2, aggregateHistory.AggregateVersions.Count);
            Assert.Equal(2, aggregateHistory.AggregateVersions.ToList()[1].VersionId);
            Assert.Equal(this.unitOfWorkId, aggregateHistory.AggregateVersions.ToList()[1].UnitWorkId);
            Assert.Equal(
                typeof(Car).AssemblyQualifiedName,
                aggregateHistory.AggregateVersions.ToList()[1].AssemblyQualifiedTypeName);
        }

        [Fact]
        public void ItShouldCreateTheAggregateHistoryItemRecord()
        {
            Assert.Equal(2, this.testHarness.DataStore.ExecutedOperations.Count(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var aggregateHistoryItems = this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().ToList();

            Assert.Equal(2, aggregateHistoryItems.Count());
            Assert.Equal("Volvo", aggregateHistoryItems.ToList()[0].AggregateVersion.Make);
            Assert.Equal("Ford", aggregateHistoryItems.ToList()[1].AggregateVersion.Make);
            Assert.NotEqual(Guid.Empty, aggregateHistoryItems.ToList()[1].id);
        }

        [Fact]
        public void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            Assert.Equal(
                this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().ToList()[1].id,
                this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.ToList()[1].AggegateHistoryItemId);
        }

        [Fact]
        public void ItShouldUpdateTheAggregateHistoryRecord()
        {
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistory.id);
            Assert.Equal(this.carId, aggregateHistory.AggregateId);
            Assert.Equal(2, aggregateHistory.Version);
        }
    }
}