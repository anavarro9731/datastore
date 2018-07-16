namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdWithVersionHistoryEnabled
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        private readonly Guid unitOfWorkId;

        public WhenCallingDeleteSoftByIdWithVersionHistoryEnabled()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteSoftByIdWithVersionHistoryEnabled),
                new DataStoreOptions
                {
                    UnitOfWorkId = this.unitOfWorkId,
                    UseVersionHistory = true
                });

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                }).Wait();

            this.testHarness.DataStore.CommitChanges().Wait();

            //When
            this.testHarness.DataStore.DeleteSoftById<Car>(this.carId).Wait();
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
            Assert.True(aggregateHistoryItems.ToList()[0].AggregateVersion.Active);
            Assert.False(aggregateHistoryItems.ToList()[1].AggregateVersion.Active);
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