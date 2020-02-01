namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoft
    {
        private Car originalCar;

        private Car updatedCar;

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoft));

            this.originalCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(this.originalCar);

            //When
            this.updatedCar = await this.testHarness.DataStore.DeleteSoft(this.originalCar);
            await this.testHarness.DataStore.CommitChanges();

            this.versionHistory = this.testHarness.QueryDatabase<Car>(cars => cars.Where(c => c.id == this.originalCar.id)).Single().VersionHistory;

        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car> && e.MethodCalled == nameof(DataStore.DeleteSoft)));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.originalCar.id)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldUpdateTheEtagsCorrectly()
        {
            await Setup();
            Assert.NotEmpty(this.originalCar.Etag); //- it was set using callback
            Assert.NotEqual(this.originalCar.Etag, this.updatedCar.Etag);
        }


        [Fact]
        public async void ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistory);
            Assert.Matches("^[0-9]*$", this.versionHistory.Single().UnitOfWorkId);
        }
    }
}