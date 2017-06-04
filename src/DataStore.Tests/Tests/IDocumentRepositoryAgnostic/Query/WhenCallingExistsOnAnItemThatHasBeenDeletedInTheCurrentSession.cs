namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession
    {
        public WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession()
        {
            // Given
            testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(activeExistingCar);

            // When
            testHarness.DataStore.DeleteHardById<Car>(activeCarId).Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid activeCarId;

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(testHarness.DataStore.ReadActiveById<Car>(activeCarId).Result);
        }
    }
}