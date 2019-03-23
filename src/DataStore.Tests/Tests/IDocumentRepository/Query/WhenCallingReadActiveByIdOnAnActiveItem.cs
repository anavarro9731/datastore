namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnActiveItem
    {
        private readonly Car activeCarFromDatabase;

        private readonly Guid activeCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingReadActiveByIdOnAnActiveItem()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnActiveItem));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = this.activeCarId,
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
            this.activeCarFromDatabase = this.testHarness.DataStore.ReadActiveById<Car>(this.activeCarId).Result;
        }

        [Fact]
        public void ItShouldReturnTheItem()
        {
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Equal(this.activeCarId, this.activeCarFromDatabase.id);
        }
    }
}