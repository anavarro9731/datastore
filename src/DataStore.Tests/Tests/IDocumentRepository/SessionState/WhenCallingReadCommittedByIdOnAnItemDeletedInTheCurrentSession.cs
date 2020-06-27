namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Newtonsoft.Json;
    using Xunit;

    public class WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession
    {
        private Car carFromDatabase;

        private Guid carId;

        [Fact]
        public async void ItShouldReturnTheItem()
        {
            await Setup();
            Assert.Equal("Volvo", this.carFromDatabase.Make);
            Assert.Equal(this.carId, this.carFromDatabase.id);
        }

        private async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId, Active = false, Make = "Volvo"
            };
            testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            await testHarness.DataStore.DeleteById<Car>(this.carId);

            // When
            var document = await testHarness.DataStore.WithoutEventReplay.ReadById<Car>(this.carId);
            try
            {
                this.carFromDatabase = document; //this approach is for Azure
            }
            catch (Exception)
            {
                this.carFromDatabase = JsonConvert.DeserializeObject<Car>(JsonConvert.SerializeObject(document));
            }
        }
    }
}