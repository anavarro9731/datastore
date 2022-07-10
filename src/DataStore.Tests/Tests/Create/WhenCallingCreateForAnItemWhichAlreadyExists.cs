namespace DataStore.Tests.Tests.Create
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateForAnItemWhichAlreadyExists
    {
        private Exception exception;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldThrowAnError()
        {
            await Setup();
            Assert.Contains("cfe3ebc2-4677-432b-9ded-0ef498b9f59d", this.exception.Message);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateForAnItemWhichAlreadyExists));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();

            //when
            this.exception = await Assert.ThrowsAnyAsync<Exception>(async () => await this.testHarness.DataStore.Create(newCar));
        }
    }
}