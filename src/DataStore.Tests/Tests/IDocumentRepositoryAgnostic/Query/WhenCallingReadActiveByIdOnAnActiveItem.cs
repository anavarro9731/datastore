namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnActiveItem
    {
        public WhenCallingReadActiveByIdOnAnActiveItem()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnActiveItem));

            activeCarId = Guid.NewGuid();
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
            activeCarFromDatabase = testHarness.DataStore.ReadActiveById<Car>(activeCarId).Result;
        }

        private readonly ITestHarness testHarness;
        private readonly Car activeCarFromDatabase;
        private readonly Guid activeCarId;

        [Fact]
        public void ItShouldReturnTheItem()
        {
            Assert.Equal(1, testHarness.DataStore.ExecutedOperations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Equal(activeCarId, activeCarFromDatabase.id);
        }
    }
}