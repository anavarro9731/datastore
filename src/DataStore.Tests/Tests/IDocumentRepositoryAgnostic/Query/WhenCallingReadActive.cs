namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadActive
    {
        public WhenCallingReadActive()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActive));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            activeCarFromDatabase = testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == activeCarId))
                .Result.SingleOrDefault();
            inactiveCarFromDatabase = testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == inactiveCarId))
                .Result.SingleOrDefault();
        }

        private readonly ITestHarness testHarness;
        private readonly Car activeCarFromDatabase;
        private readonly Car inactiveCarFromDatabase;

        [Fact]
        public void ItShouldOnlyReturnActiveItems()
        {
            Assert.Equal(2, testHarness.DataStore.ExecutedOperations.Count(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal("Volvo", activeCarFromDatabase.Make);
            Assert.Null(inactiveCarFromDatabase);
        }
    }
}