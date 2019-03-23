namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithSkipAndTake
    {
        private readonly IEnumerable<Car> carsFromDatabase;

        private readonly Guid fourthCarId;

        private readonly ITestHarness testHarness;

        private readonly Guid thirdCarId;

        public WhenCallingReadActiveWithSkipAndTake()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveWithSkipAndTake));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Ford"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Volvo"
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
            this.carsFromDatabase = this.testHarness.DataStore.WithoutEventReplay
                                        .ReadActive<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(1).Take(2)).Result;
        }

        [Fact]
        public void ItShouldReturnTheLastTwoVolvos()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Single(this.carsFromDatabase);
            Assert.Equal(this.fourthCarId, this.carsFromDatabase.Single().id);
        }
    }
}