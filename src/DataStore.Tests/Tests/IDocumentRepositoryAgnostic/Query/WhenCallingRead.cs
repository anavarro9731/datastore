namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingRead
    {
        public WhenCallingRead()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingRead));

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
                Make = "Volvo"
            };
            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            carsFromDatabase = testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.Make == "Volvo")).Result;
        }

        private readonly ITestHarness testHarness;
        private readonly IEnumerable<Car> carsFromDatabase;

        [Fact]
        public void ItShouldReturnAllItemsRegardlessOfActiveFlag()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, carsFromDatabase.Count());
        }
    }
}