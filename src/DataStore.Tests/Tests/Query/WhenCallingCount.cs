namespace DataStore.Tests.Tests.Query
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCount
    {
        private int countOfCars;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnACountOf2()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregateCountedOperation<Car>));
            Assert.Equal(2, this.countOfCars);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCount));

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
            this.countOfCars = await this.testHarness.DataStore.WithoutEventReplay.Count<Car>(car => car.Make == "Volvo");
        }
    }
}