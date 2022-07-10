namespace DataStore.Tests.Tests.Read
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnActiveItem
    {
        private Car activeCarFromDatabase;

        private Guid activeCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnTheItem()
        {
            await Setup();
            Assert.Equal(
                1,
                this.testHarness.DataStore.ExecutedOperations.Count(
                    e => e is IDataStoreReadByIdOperation && e.MethodCalled == nameof(DataStore.ReadActiveById)));
            Assert.Equal(this.activeCarId, this.activeCarFromDatabase.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnActiveItem));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = this.activeCarId, Active = true, Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId, Active = false, Make = "Jeep"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(inactiveExistingCar);

            // When
            this.activeCarFromDatabase = await this.testHarness.DataStore.ReadActiveById<Car>(this.activeCarId);
        }
    }
}