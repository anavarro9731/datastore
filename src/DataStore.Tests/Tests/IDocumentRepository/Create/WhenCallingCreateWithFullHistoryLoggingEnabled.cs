namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithFullHistoryLoggingEnabled
    {
        private Car newCar;

        private Guid newCarId;

        private ITestHarness testHarness;

        private Guid unitOfWorkId;

        [Fact]
        public async void ItShouldAddAHistoryIndexEntityToTheHistory()
        {
            await Setup();

            Assert.Single(this.newCar.VersionHistory);
            var aggregateVersionInfo = this.newCar.VersionHistory.Single();
            Assert.Equal(this.unitOfWorkId.ToString(), aggregateVersionInfo.UnitOfWorkId);
            Assert.Equal(1, aggregateVersionInfo.CommitBatch);
        }

        [Fact]
        public async void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var aggregateHistoryItem = this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>().Single();
            Assert.NotEqual(Guid.Empty, aggregateHistoryItem.id);
            Assert.True(aggregateHistoryItem.AggregateVersion.id == this.newCarId);
        }

        [Fact]
        public async void ItShouldCreateTheCorrectReferenceBetweenTheTwoRecords()
        {
            await Setup();

            Assert.Equal(
                this.newCar.VersionHistory.Single().AggegateHistoryItemId,
                this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>().Single().id);
        }

        private async Task Setup()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithFullHistoryLoggingEnabled),
                DataStoreOptions.Create().EnableFullVersionHistory().SpecifyUnitOfWorkId(this.unitOfWorkId));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            //When
            await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();
            this.newCar = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.newCarId)).Single();
        }
    }
}