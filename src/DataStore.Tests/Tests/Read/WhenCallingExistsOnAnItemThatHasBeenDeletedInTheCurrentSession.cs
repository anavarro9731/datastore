namespace DataStore.Tests.Tests.Read
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession
    {
        private Guid activeCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(await this.testHarness.DataStore.ReadActiveById<Car>(this.activeCarId));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = this.activeCarId, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);

            // When
            await this.testHarness.DataStore.DeleteById<Car>(this.activeCarId);
        }
    }
}