namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdWithVersionHistoryEnabled
    {
        private  Guid carId;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.carId = Guid.NewGuid();


            this.testHarness = TestHarness
                .Create(nameof(WhenCallingDeleteSoftByIdWithVersionHistoryEnabled), 
                    DataStoreOptions.Create().EnableFullVersionHistory());

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
        public async void ItShouldAddAHistoryIndexEntityToTheHistory()
        {
            await Setup();

            var car = this.testHarness.QueryDatabase<Car>(cars => cars.Where(c => c.id == this.carId)).Single();
            Assert.Equal(2, car.VersionHistory.Count);
            var aggregateVersionInfo = car.VersionHistory.First();
            Assert.Matches("^[0-9]*$", aggregateVersionInfo.UnitOfWorkId);
            Assert.Equal(2, aggregateVersionInfo.CommitBatch);
        }

        [Fact]
        public async void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            await Setup();

            //- create for create and update ops
            Assert.Equal(2, this.testHarness.DataStore
                               .ExecutedOperations.Count(e => e is CreateOperation<AggregateHistoryItem<Car>>));

            var car = this.testHarness.QueryDatabase<Car>(cars => cars.Where(c => c.id == this.carId)).Single();
            var aggregateVersionInfo = car.VersionHistory.First();

            var aggregateHistoryItem = this.testHarness
                                           .QueryDatabase<AggregateHistoryItem<Car>>(cars => cars.Where(x => x.id == aggregateVersionInfo.AggegateHistoryItemId))
                                           .SingleOrDefault();

            Assert.NotNull(aggregateHistoryItem);
            Assert.Equal(typeof(Car).AssemblyQualifiedName, aggregateHistoryItem.AssemblyQualifiedTypeName);

            Assert.True(aggregateHistoryItem.AggregateVersion.id == this.carId);
        }


    }
}