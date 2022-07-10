namespace DataStore.Tests.Tests.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingRead
    {
        private IEnumerable<Car> carsFromDatabase;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnAllItemsRegardlessOfActiveFlag()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.Read)));
            Assert.Equal(2, this.carsFromDatabase.Count());
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingRead));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId, Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId, Active = false, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(inactiveExistingCar);

            // When

            this.carsFromDatabase = await this.testHarness.DataStore.Read<Car>(car => car.Make == "Volvo");
        }
    }
}