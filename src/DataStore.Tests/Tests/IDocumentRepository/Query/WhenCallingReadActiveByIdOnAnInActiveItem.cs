namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnInactiveItem
    {
        private Car inactiveCarFromDatabase;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnInactiveItem));

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
            this.inactiveCarFromDatabase = await this.testHarness.DataStore.ReadActiveById<Car>(inactiveCarId);
        }

        [Fact]
        public async void ItShouldNotReturnTheItem()
        {
            await Setup();
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Null(this.inactiveCarFromDatabase);
        }
    }
}