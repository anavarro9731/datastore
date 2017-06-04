namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession
    {
        public WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession));

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
            carFromDatabase = testHarness.DataStore.Advanced
                .ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId))
                .Result.Single();
        }

        private readonly ITestHarness testHarness;
        private readonly Car carFromDatabase;

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.NotNull(carFromDatabase);
        }
    }
}