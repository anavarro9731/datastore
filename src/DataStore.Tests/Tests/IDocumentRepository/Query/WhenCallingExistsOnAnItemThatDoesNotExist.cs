namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatDoesNotExist
    {
        private readonly bool carExists;

        public WhenCallingExistsOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingExistsOnAnItemThatDoesNotExist));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(activeExistingCar);

            // When
            this.carExists = testHarness.DataStore.Exists(Guid.NewGuid()).Result;
        }

        [Fact]
        public void ItShouldReturnFalseIfTheItemDoesNotExist()
        {
            Assert.False(this.carExists);
        }
    }
}