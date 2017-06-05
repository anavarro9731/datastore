using System;
using System.Linq;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    public class WhenCallingReadActive
    {
        public WhenCallingReadActive()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActive));

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

        private readonly Car activeCarFromDatabase;
        private readonly Car inactiveCarFromDatabase;

        [Fact]
        public void ItShouldNotReturnInActiveItems()
        {
            Assert.Null(inactiveCarFromDatabase);
        }

        [Fact]
        public void ItShouldReturnActiveItems()
        {
            Assert.Equal("Volvo", activeCarFromDatabase.Make);
        }
    }
}