namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        private readonly Exception exception;

        public WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            this.testHarness.DataStore.Create(newCar).Wait();

            //When
            this.exception = Assert.ThrowsAny<Exception>(() => this.testHarness.DataStore.Create(newCar).Wait());
        }

        [Fact]
        public void ItShouldErrorWhenYouCreateTheSecondTime()
        {
           Assert.Contains("63328bcd-d58d-446a-bc85-fedfde43d2e2", this.exception.InnerException.Message);
        }
    }
}