namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnInactiveItem
    {
        private readonly bool activeCarExists;

        private readonly bool inactiveCarExists;

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
            this.activeCarExists = testHarness.DataStore.Exists(activeCarId).Result;
            this.inactiveCarExists = testHarness.DataStore.Exists(inactiveCarId).Result;
        }

        [Fact]
        public void ItShouldReturnTheItem()
        {
            Assert.True(this.inactiveCarExists);
            Assert.True(this.activeCarExists);
        }
    }
}