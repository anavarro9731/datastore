namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Data;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdateWithAnOutdatedEtag
    {
        private Guid carId;

        private Car existingCar;

        private ITestHarness testHarness;

        private Car udpatedCar;

        [Fact]
        public async void ItShouldThrowAConcurrencyException()
        {
            await Setup();
            await Assert.ThrowsAnyAsync<DBConcurrencyException>(async () => await this.testHarness.DataStore.CommitChanges());
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWithAnOutdatedEtag));

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

            existingCarFromDb.Etag = "fake outdated tag";

            this.udpatedCar = await this.testHarness.DataStore.Update(existingCarFromDb);
        }
    }
}
