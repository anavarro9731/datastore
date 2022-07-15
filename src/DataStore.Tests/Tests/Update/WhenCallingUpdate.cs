namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdate
    {
        private Guid carId;

        private Car existingCar;

        private ITestHarness testHarness;

        private Car udpatedCar;

        private Guid unitOfWorkId = Guid.NewGuid();

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistory);
            Assert.Equal(this.unitOfWorkId.ToString(), this.versionHistory.Single().UnitOfWorkId);
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is UpdateOperation<Car> && e.MethodCalled == nameof(DataStore.Update)));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(
                "Ford",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }

        [Fact]
        public async void ItShouldUpdateTheEtagsCorrectly()
        {
            await Setup();
            Assert.NotEmpty(this.existingCar.Etag); //- it was set using callback
            Assert.NotEmpty(this.udpatedCar.Etag); //- it was set using callback
            Assert.NotEqual(this.existingCar.Etag, this.udpatedCar.Etag);
        }

        [Fact]
        public async void ItShouldUpdateTheModifiedDate()
        {
            await Setup();
            Assert.NotEqual(this.existingCar.Modified.Date, this.udpatedCar.Modified.Date);
            Assert.NotEqual(this.existingCar.ModifiedAsMillisecondsEpochTime, this.udpatedCar.ModifiedAsMillisecondsEpochTime);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdate),
                DataStoreOptions.Create().SpecifyUnitOfWorkId(this.unitOfWorkId));

            this.carId = Guid.NewGuid();

            this.existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToMillisecondsEpochTime()
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(this.existingCar);

            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);

            existingCarFromDb.Make = "Ford";

            //When
            this.udpatedCar = await this.testHarness.DataStore.Update(existingCarFromDb);
            await this.testHarness.DataStore.CommitChanges();

            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single()
                                      .VersionHistory;
        }
    }
}
