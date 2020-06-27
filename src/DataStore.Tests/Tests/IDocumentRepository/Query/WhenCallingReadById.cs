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

    public class WhenCallingReadById
    {
        private Car inactiveCarFromDatabase;

        private Guid inactiveCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnTheItem()
        {
            await Setup();
            Assert.Equal(
                1,
                this.testHarness.DataStore.ExecutedOperations.Count(
                    e => e is AggregateQueriedByIdOperation && e.MethodCalled == nameof(DataStore.ReadById)));
            Assert.Equal(this.inactiveCarId, this.inactiveCarFromDatabase.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadById));

            this.inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = this.inactiveCarId, Active = false, Make = "Jeep"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(inactiveExistingCar);

            // When
            this.inactiveCarFromDatabase = await this.testHarness.DataStore.ReadById<Car>(this.inactiveCarId);
        }
    }
}