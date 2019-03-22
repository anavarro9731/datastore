namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCountActive
    {
        private readonly int countOfCars;

        private readonly ITestHarness testHarness;

        public WhenCallingCountActive()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCountActive));

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
            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);

            // When
            this.countOfCars = this.testHarness.DataStore.WithoutEventReplay.CountActive<Car>(car => car.Make == "Volvo").Result;
        }

        [Fact]
        public void ItShouldReturnACountOf1()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregateCountedOperation<Car>));
            Assert.Equal(1, this.countOfCars);
        }
    }
}