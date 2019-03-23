namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActive
    {
        private readonly Car activeCarFromDatabase;

        private readonly Car inactiveCarFromDatabase;

        public WhenCallingReadActive()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActive));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                Id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                Id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            this.activeCarFromDatabase = testHarness.DataStore.ReadActive<Car>(car => car.Id == activeCarId).Result.SingleOrDefault();
            this.inactiveCarFromDatabase = testHarness.DataStore.ReadActive<Car>(car => car.Id == inactiveCarId).Result.SingleOrDefault();
        }

        [Fact]
        public void ItShouldNotReturnInActiveItems()
        {
            Assert.Null(this.inactiveCarFromDatabase);
        }

        [Fact]
        public void ItShouldReturnActiveItems()
        {
            Assert.Equal("Volvo", this.activeCarFromDatabase.Make);
        }
    }
}