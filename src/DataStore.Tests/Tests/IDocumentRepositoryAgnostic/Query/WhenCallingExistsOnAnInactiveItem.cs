namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnInactiveItem
    {
        public WhenCallingExistsOnAnInactiveItem()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExistsOnAnInactiveItem));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            activeCarExists = testHarness.DataStore.Exists(activeCarId).Result;
            inactiveCarExists = testHarness.DataStore.Exists(inactiveCarId).Result;
        }

        private readonly bool activeCarExists;
        private readonly bool inactiveCarExists;

        [Fact]
        public void ItShouldReturnTheItem()
        {
            //Then
            Assert.True(inactiveCarExists);
            Assert.True(activeCarExists);
        }
    }
}