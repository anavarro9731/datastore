namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateForAnItemWhichAlreadyExists
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        private readonly Exception exception;

        public WhenCallingCreateForAnItemWhichAlreadyExists()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreate));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                Id = this.newCarId,
                Make = "Volvo"
            };

            this.testHarness.DataStore.Create(newCar).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();

            //when
            this.exception = Assert.ThrowsAny<Exception>(() => this.testHarness.DataStore.Create(newCar).Wait());

        }

        [Fact]
        public void ItShouldThrowAnError()
        {
            Assert.Contains("cfe3ebc2-4677-432b-9ded-0ef498b9f59d", this.exception.InnerException.Message);
        }
    }
}