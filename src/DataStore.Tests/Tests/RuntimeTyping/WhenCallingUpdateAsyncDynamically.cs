namespace DataStore.Tests.Tests.RuntimeTyping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateAsyncDynamically
    {
        private Guid carId;

        private Car existingCar;

        private ITestHarness testHarness;

        private Guid unitOfWorkId = Guid.NewGuid();

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact(
            Skip = @"this test makes clear an issue where this.result doesn't include new 
        but uncommitted history this is an interesting issue which is not really a bug because 
        history is reloaded each operation but does leave the caller depending on their expectation
        with an outdated model?")]
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
            Assert.Equal(
                "Ford",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateAsyncDynamically),
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

            await this.testHarness.DataStore.DocumentRepository.UpdateAsync(existingCarFromDb);

            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single()
                                      .VersionHistory;
        }
    }
}
