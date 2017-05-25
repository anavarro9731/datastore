namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession
    {
        public WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(activeExistingCar);

            testHarness.DataStore.DeleteHardById<Car>(activeCarId).Wait();

            // When
            activeCarFromDataStore = testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == activeCarId))
                .Result.SingleOrDefault();
        }

        private readonly Car activeCarFromDataStore;

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(activeCarFromDataStore);
        }
    }
}