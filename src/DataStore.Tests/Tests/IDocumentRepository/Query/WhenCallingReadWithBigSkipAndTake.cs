namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithBigSkipAndTake
    {
        private  IEnumerable<Car> carsFromDatabaseWithFilter1;
        private  IEnumerable<Car> carsFromDatabaseWithFilter2;
        private  IEnumerable<Car> carsFromDatabaseWithFilter3;
        private  IEnumerable<Car> carsFromDatabaseWithFilter4;

        private  IEnumerable<Car> carsInDatabase;

        private  Guid fourthCarId;

        private  ITestHarness testHarness;

        private  Guid thirdCarId;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithBigSkipAndTake));


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
            this.carsFromDatabaseWithFilter1 =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(2101));
            this.carsFromDatabaseWithFilter2 =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(3000));
            this.carsFromDatabaseWithFilter3 =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(2500));
            this.carsFromDatabaseWithFilter4 =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(car => car.Make == "Volvo", o => o.Skip(100).Take(1500));

        }

        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeMatchesMaximumResults()
        {
            await Setup();
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter1.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithNoRemainderFrom1000MaxTakeInRepo()
        {
            await Setup();
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter2.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithRemainderFrom1000MaxTakeInRepo()
        {
            await Setup();
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter3.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }
        [Fact]
        public async void ItShouldReturnTheCorrectAmountOfVolvosWhenTakeIsLessThanAmountOfVolvosAvailableAfterSKip()
        {
            await Setup();
            Assert.True(this.testHarness.DataStore.ExecutedOperations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(1500, this.carsFromDatabaseWithFilter4.Count()); 
        }
    }
}