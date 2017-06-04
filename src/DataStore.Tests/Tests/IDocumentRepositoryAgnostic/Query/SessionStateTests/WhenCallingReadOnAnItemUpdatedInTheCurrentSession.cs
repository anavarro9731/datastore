namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadOnAnItemUpdatedInTheCurrentSession
    {
        public WhenCallingReadOnAnItemUpdatedInTheCurrentSession()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingReadOnAnItemUpdatedInTheCurrentSession));

            carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford").Wait();

            // When
            carFromSession = testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId)).Result.Single();
        }

        private readonly ITestHarness testHarness;
        private readonly Car carFromSession;
        private readonly Guid carId;

        [Fact]
        public void ItShouldReturnTheItemWithTheUpdatesApplied()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
            Assert.Equal("Ford", carFromSession.Make);
        }
    }
}