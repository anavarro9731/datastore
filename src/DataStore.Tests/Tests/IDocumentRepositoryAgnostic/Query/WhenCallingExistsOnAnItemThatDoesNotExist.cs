namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatDoesNotExist
    {
        public WhenCallingExistsOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExistsOnAnItemThatDoesNotExist));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(activeExistingCar);

            // When
            carExists = testHarness.DataStore.Exists(Guid.NewGuid()).Result;
        }

        private readonly bool carExists;

        [Fact]
        public void ItShouldReturnFalseIfTheItemDoesNotExist()
        {
            Assert.False(carExists);
        }
    }
}