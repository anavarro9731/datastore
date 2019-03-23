namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithSkipAndTake
    {
        private readonly IEnumerable<Car> carsFromDatabase;

        private readonly Guid fourthCarId;

        private readonly ITestHarness testHarness;

        private readonly Guid thirdCarId;

        public WhenCallingReadWithSkipAndTake()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithSkipAndTake));

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
                Make = "Ford"
            };

            this.thirdCarId = Guid.NewGuid();
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId,
                Active = true,
                Make = "Volvo"
            };

            this.fourthCarId = Guid.NewGuid();
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId,
                Active = true,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);
            this.testHarness.AddToDatabase(thirdExistingCar);
            this.testHarness.AddToDatabase(fourthExistingCar);

            // When
            this.carsFromDatabase = this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(1).Take(2)).Result;
        }

        [Fact]
        public void ItShouldReturnTheLastTwoVolvos()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, this.carsFromDatabase.Count());
            Assert.Equal(this.thirdCarId, this.carsFromDatabase.First().id);
            Assert.Equal(this.fourthCarId, this.carsFromDatabase.Last().id);
        }
    }
}