namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession
    {
        public WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession()
        {
            // Given
            testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.DeleteSoftById<Car>(carId).Wait();

            // When
            carFromSession = testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == carId))
                .Result.SingleOrDefault();
        }

        private readonly Car carFromSession;
        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldNotReturnThatItem()
        {
            Assert.NotNull(testHarness.Operations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Count());
            Assert.Null(carFromSession);
        }
    }
}