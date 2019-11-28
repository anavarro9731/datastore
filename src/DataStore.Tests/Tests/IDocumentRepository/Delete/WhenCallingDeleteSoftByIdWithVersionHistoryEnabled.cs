namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdWithVersionHistoryEnabled
    {
        private  Guid carId;

        private  ITestHarness testHarness;

        private  Guid unitOfWorkId;

        async Task Setup()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteSoftByIdWithVersionHistoryEnabled),
                DataStoreOptions.Create().WithVersionHistory(this.unitOfWorkId));

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();

            //When
            await this.testHarness.DataStore.DeleteSoftById<Car>(this.carId);
            await this.testHarness.DataStore.CommitChanges();
        }


        [Fact]
        public async void ItShouldAddAHistoryIndexEntityToTheHistoryAggregate()
        {
            await Setup();
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

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
            Assert.True(aggregateHistoryItems.ToList()[0].AggregateVersion.Active);
            Assert.False(aggregateHistoryItems.ToList()[1].AggregateVersion.Active);
            Assert.NotEqual(Guid.Empty, aggregateHistoryItems.ToList()[1].id);
        }

        [Fact]
        public async void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            await Setup();
            Assert.Equal(
                this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>().ToList()[1].id,
                this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.ToList()[1].AggegateHistoryItemId);
        }

        [Fact]
        public async void ItShouldUpdateTheAggregateHistoryRecord()
        {
            await Setup();
            var aggregateHistory = this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single();

            Assert.NotEqual(Guid.Empty, aggregateHistory.id);
            Assert.Equal(this.carId, aggregateHistory.AggregateId);
            Assert.Equal(2, aggregateHistory.Version);
        }
    }
}