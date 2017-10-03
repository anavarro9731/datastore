namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Newtonsoft.Json;
    using Xunit;

    public class WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession
    {
        private readonly Car carFromDatabase;

        private readonly Guid carId;

        public WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.DeleteHardById<Car>(this.carId).Wait();

            // When
            var document = testHarness.DataStore.Advanced.ReadCommittedById<Car>(this.carId).Result;
            try
            {
                this.carFromDatabase = document; //this approach is for Azure
            }
            catch (Exception)
            {
                this.carFromDatabase = JsonConvert.DeserializeObject<Car>(JsonConvert.SerializeObject(document));
            }
        }

        [Fact]
        public void ItShouldReturnTheItem()
        {
            Assert.Equal("Volvo", this.carFromDatabase.Make);
            Assert.Equal(this.carId, this.carFromDatabase.id);
        }
    }
}