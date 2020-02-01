namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession
    {
        private Car carFromDatabase;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(existingCar);

            await this.testHarness.DataStore.DeleteHardById<Car>(carId);

            // When
            this.carFromDatabase = (await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(car => car.id == carId)).Single();
        }

        [Fact]
        public async void ItShouldReturnThatItem()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.Where(e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.Read)));
            Assert.NotNull(this.carFromDatabase);
        }
    }
}