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

    public class WhenCallingCreateForAnItemWhichAlreadyExists
    {
        private CircuitException exception;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldThrowAnError()
        {
            await Setup();
            Assert.Equal("cfe3ebc2-4677-432b-9ded-0ef498b9f59d", this.exception.Error.Key);
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
            this.exception = await Assert.ThrowsAnyAsync<CircuitException>(async () => await this.testHarness.DataStore.Create(newCar));
        }
    }
}