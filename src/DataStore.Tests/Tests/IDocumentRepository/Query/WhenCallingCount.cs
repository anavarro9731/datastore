namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCount
    {
        private readonly int countOfCars;

        private readonly ITestHarness testHarness;

        public WhenCallingCount()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCount));

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
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);

            // When
            this.countOfCars = this.testHarness.DataStore.WithoutEventReplay.Count<Car>(car => car.Make == "Volvo").Result;
        }

        [Fact]
        public void ItShouldReturnACountOf2()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregateCountedOperation<Car>));
            Assert.Equal(2, this.countOfCars);
        }
    }
}