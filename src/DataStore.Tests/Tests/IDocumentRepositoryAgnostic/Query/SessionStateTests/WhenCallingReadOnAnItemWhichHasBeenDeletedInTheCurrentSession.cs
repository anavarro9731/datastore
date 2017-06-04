namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession
    {
        public WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession()
        {
            // Given
            testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            // When
            carFromSession = testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))
                .Result.SingleOrDefault();
        }

        private readonly Car carFromSession;
        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldNotReturnThatItem()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Count());
            Assert.Null(carFromSession);
        }
    }
}