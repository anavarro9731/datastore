namespace DataStore.Tests.Tests.Create
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued
    {
        private Exception exception;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldErrorWhenYouCreateTheSecondTime()
        {
            await Setup();
            Assert.Contains("63328bcd-d58d-446a-bc85-fedfde43d2e2", this.exception.Message);
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
            this.exception = await Assert.ThrowsAnyAsync<Exception>(async () => await this.testHarness.DataStore.Create(newCar));
        }
    }
}