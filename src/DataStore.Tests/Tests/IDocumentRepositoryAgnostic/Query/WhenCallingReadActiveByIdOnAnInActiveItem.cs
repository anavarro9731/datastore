namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnInactiveItem
    {
        public WhenCallingReadActiveByIdOnAnInactiveItem()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnInactiveItem));

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

            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            inactiveCarFromDatabase = testHarness.DataStore.ReadActiveById<Car>(inactiveCarId).Result;
        }

        private readonly ITestHarness testHarness;
        private readonly Car inactiveCarFromDatabase;

        [Fact]
        public void ItShouldNotReturnTheItem()
        {
            Assert.Equal(1, testHarness.DataStore.ExecutedOperations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Null(inactiveCarFromDatabase);
        }
    }
}