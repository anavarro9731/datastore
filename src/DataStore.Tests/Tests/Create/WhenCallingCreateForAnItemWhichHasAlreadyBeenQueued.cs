namespace DataStore.Tests.Tests.Create
{
    #region

    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued
    {
        private CircuitException exception;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldErrorWhenYouCreateTheSecondTime()
        {
            await Setup();
            Assert.Equal("63328bcd-d58d-446a-bc85-fedfde43d2e2", this.exception.Error.Key);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            await this.testHarness.DataStore.Create(newCar);

            //When
            this.exception = await Assert.ThrowsAnyAsync<CircuitException>(async () => await this.testHarness.DataStore.Create(newCar));
        }
    }
}