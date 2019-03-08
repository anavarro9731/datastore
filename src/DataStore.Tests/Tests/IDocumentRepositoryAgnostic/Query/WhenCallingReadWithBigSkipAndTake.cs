namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithBigSkipAndTake
    {
        private readonly IEnumerable<Car> carsFromDatabaseWithFilter1;
        private readonly IEnumerable<Car> carsFromDatabaseWithFilter2;
        private readonly IEnumerable<Car> carsFromDatabaseWithFilter3;

        private readonly IEnumerable<Car> carsInDatabase;

        private readonly Guid fourthCarId;

        private readonly ITestHarness testHarness;

        private readonly Guid thirdCarId;

        public WhenCallingReadWithBigSkipAndTake()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadWithBigSkipAndTake));


            Enumerable.Range(1, 1000).ToList().ForEach(
                    i =>
                        {
                            var activeCarId = Guid.NewGuid();
                            var activeExistingCar = new Car
                            {
                                id = activeCarId,
                                Make = "Volvo"
                            };
                            this.testHarness.AddToDatabase(activeExistingCar);
                        });

            Enumerable.Range(1, 1200).ToList().ForEach(
                i =>
                    {
                        var inactiveCarId = Guid.NewGuid();
                        var inactiveExistingCar = new Car
                        {
                            id = inactiveCarId,
                            Active = false,
                            Make = "Volvo"
                        };
                        this.testHarness.AddToDatabase(inactiveExistingCar);
                    });

            this.thirdCarId = Guid.NewGuid();
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId,
                Active = true,
                Make = "Ford"
            };

            this.fourthCarId = Guid.NewGuid();
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId,
                Active = true,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(thirdExistingCar);
            this.testHarness.AddToDatabase(fourthExistingCar);

            // When
            this.carsInDatabase = this.testHarness.QueryDatabase<Car>();
            this.carsFromDatabaseWithFilter1 = this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(2101)).Result;
            this.carsFromDatabaseWithFilter2 = this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(3000)).Result;
            this.carsFromDatabaseWithFilter3 = this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(2500)).Result;
        }

        [Fact]
        public void ItShouldReturnAllVolvosWhenTakeMatchesMaximumResults()
        {
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter1.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
        [Fact]
        public void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithNoRemainderFrom1000MaxTakeInRepo()
        {
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter2.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
        [Fact]
        public void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithRemainderFrom1000MaxTakeInRepo()
        {
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter3.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
    }
}