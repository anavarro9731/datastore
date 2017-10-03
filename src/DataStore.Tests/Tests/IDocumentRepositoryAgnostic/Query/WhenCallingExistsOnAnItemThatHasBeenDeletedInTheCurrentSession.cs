namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
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
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = this.activeCarId,
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