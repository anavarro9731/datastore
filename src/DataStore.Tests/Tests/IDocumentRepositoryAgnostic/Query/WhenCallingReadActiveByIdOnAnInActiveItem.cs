namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnInactiveItem
    {
        private readonly Car inactiveCarFromDatabase;

        private readonly ITestHarness testHarness;

        public WhenCallingReadActiveByIdOnAnInactiveItem()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnInactiveItem));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Active = true,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };

            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);

            // When
            this.inactiveCarFromDatabase = this.testHarness.DataStore.ReadActiveById<Car>(inactiveCarId).Result;
        }

        [Fact]
        public void ItShouldNotReturnTheItem()
        {
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Null(this.inactiveCarFromDatabase);
        }
    }
}