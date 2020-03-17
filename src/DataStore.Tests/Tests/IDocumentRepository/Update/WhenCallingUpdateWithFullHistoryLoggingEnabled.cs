namespace DataStore.Tests.Tests.IDocumentRepository.Update
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

    public class WhenCallingUpdateWithFullHistoryLoggingEnabled
    {
        private Guid carId;

        private ITestHarness testHarness;

        private Guid unitOfWorkId;

        async Task Setup()
        {
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWithFullHistoryLoggingEnabled),
                DataStoreOptions.Create().EnableFullVersionHistory().SpecifyUnitOfWorkId(this.unitOfWorkId));

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
        public async void ItShouldAddAHistoryIndexEntityToTheHistory()
        {
            await Setup();

            var car = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single();
            Assert.Equal(2, car.VersionHistory.Count);
            var aggregateVersionInfo = car.VersionHistory.First();
            Assert.Equal(this.unitOfWorkId.ToString(), aggregateVersionInfo.UnitOfWorkId);
            Assert.Equal(2, aggregateVersionInfo.CommitBatch);
        }

        [Fact]
        public async void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            await Setup();

            //- one for create and one for update
            Assert.Equal(2, this.testHarness.DataStore
                               .ExecutedOperations.Count(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var car = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single();
            var aggregateVersionInfo = car.VersionHistory.First();

            var aggregateHistoryItem = this.testHarness
                                           .QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>(cars => cars.Where(x => x.id == aggregateVersionInfo.AggegateHistoryItemId))
                                           .SingleOrDefault();

            Assert.NotNull(aggregateHistoryItem);
            Assert.Equal(typeof(Car).AssemblyQualifiedName, aggregateHistoryItem.AssemblyQualifiedTypeName);

            Assert.True(aggregateHistoryItem.AggregateVersion.id == this.carId);
        }
    }
}