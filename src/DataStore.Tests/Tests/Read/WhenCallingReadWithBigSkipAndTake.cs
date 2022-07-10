namespace DataStore.Tests.Tests.Read
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithBigSkipAndTake
    {
        private IEnumerable<Car> carsFromDatabaseWithFilter1;

        private IEnumerable<Car> carsFromDatabaseWithFilter2;

        private IEnumerable<Car> carsFromDatabaseWithFilter3;

        private IEnumerable<Car> carsFromDatabaseWithFilter4;

        private IEnumerable<Car> carsInDatabase;

        private Guid fourthCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithNoRemainderFrom1000MaxTakeInRepo()
        {
            await Setup();
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter2.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }

        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeExceedsMaximumResultsWithRemainderFrom1000MaxTakeInRepo()
        {
            await Setup();
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter3.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }

        [Fact]
        public async void ItShouldReturnAllVolvosWhenTakeMatchesMaximumResults()
        {
            await Setup();
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(2101, this.carsFromDatabaseWithFilter1.Count());
            Assert.Equal(this.fourthCarId, this.carsFromDatabaseWithFilter1.Last().id);
        }

        [Fact]
        public async void ItShouldReturnTheCorrectAmountOfVolvosWhenTakeIsLessThanAmountOfVolvosAvailableAfterSKip()
        {
            await Setup();
            Assert.Equal(2202, this.carsInDatabase.Count());
            Assert.Equal(1500, this.carsFromDatabaseWithFilter4.Count());
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithBigSkipAndTake));

            Enumerable.Range(1, 1000).ToList().ForEach(
                i =>
                    {
                    var activeCarId = Guid.NewGuid();
                    var activeExistingCar = new Car
                    {
                        id = activeCarId, Make = "Volvo"
                    };
                    this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);
                    });

            Enumerable.Range(1, 1200).ToList().ForEach(
                i =>
                    {
                    var inactiveCarId = Guid.NewGuid();
                    var inactiveExistingCar = new Car
                    {
                        id = inactiveCarId, Active = false, Make = "Volvo"
                    };
                    this.testHarness.AddItemDirectlyToUnderlyingDb(inactiveExistingCar);
                    });

            this.thirdCarId = Guid.NewGuid();
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId, Active = true, Make = "Ford"
            };

            this.fourthCarId = Guid.NewGuid();
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId, Active = true, Make = "Volvo"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(thirdExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(fourthExistingCar);

            var c = new ContinuationToken();
            await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(car => car.Make == "Volvo", o => o.Take(100, ref c));

            // When
            this.carsInDatabase = this.testHarness.QueryUnderlyingDbDirectly<Car>();

            var c1 = new ContinuationToken();
            this.carsFromDatabaseWithFilter1 = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                                                   car => car.Make == "Volvo",
                                                   o => o.ContinueFrom(c).Take(2101, ref c1));

            var c2 = new ContinuationToken();
            this.carsFromDatabaseWithFilter2 = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                                                   car => car.Make == "Volvo",
                                                   o => o.ContinueFrom(c).Take(3000, ref c2));

            var c3 = new ContinuationToken();
            this.carsFromDatabaseWithFilter3 = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                                                   car => car.Make == "Volvo",
                                                   o => o.ContinueFrom(c).Take(2500, ref c3));

            var c4 = new ContinuationToken();
            this.carsFromDatabaseWithFilter4 = await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                                                   car => car.Make == "Volvo",
                                                   o => o.ContinueFrom(c).Take(1500, ref c4));
        }
    }
}