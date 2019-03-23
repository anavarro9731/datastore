namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession
    {
        private readonly Guid activeCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                Id = this.activeCarId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(activeExistingCar);

            // When
            this.testHarness.DataStore.DeleteHardById<Car>(this.activeCarId).Wait();
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.testHarness.DataStore.ReadActiveById<Car>(this.activeCarId).Result);
        }
    }
}