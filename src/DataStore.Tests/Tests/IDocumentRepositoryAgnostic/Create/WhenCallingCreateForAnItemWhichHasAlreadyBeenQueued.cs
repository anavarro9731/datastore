namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateForAnItemWhichHasAlreadyBeenQueued));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            this.testHarness.DataStore.Create(newCar).Wait();

            //When
        }

        [Fact]
        public void ItShouldErrorWhenYouCreateTheSecondTime()
        {
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            var ex = Assert.ThrowsAny<Exception>(() => this.testHarness.DataStore.Create(newCar).Wait());

            Assert.Contains("63328bcd-d58d-446a-bc85-fedfde43d2e2", ex.InnerException.Message);
        }
    }
}