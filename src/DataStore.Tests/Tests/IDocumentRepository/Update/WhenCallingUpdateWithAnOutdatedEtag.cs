namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWithAnOutdatedEtag
    {
        private Guid carId;

        private ITestHarness testHarness;

        private Car udpatedCar;

        private Car existingCar;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWithAnOutdatedEtag));

            this.carId = Guid.NewGuid();

            this.existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToSecondsEpochTime()
            };
            this.testHarness.AddToDatabase(this.existingCar);

            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);

            existingCarFromDb.Etag = "fake outdated tag";

            this.udpatedCar = await this.testHarness.DataStore.Update(existingCarFromDb);
        }

        [Fact]
        public async void ItShouldThrowAConcurrencyException()
        {
            await Setup(); 
            await Assert.ThrowsAnyAsync<Exception>(async () => await this.testHarness.DataStore.CommitChanges());
            
        }

    }
}