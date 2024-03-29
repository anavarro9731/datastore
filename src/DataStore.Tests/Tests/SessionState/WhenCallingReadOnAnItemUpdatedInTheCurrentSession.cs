namespace DataStore.Tests.Tests.SessionState
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadOnAnItemUpdatedInTheCurrentSession
    {
        private Car carFromSession;

        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnTheItemWithTheUpdatesAppliedWhenThePredicateMatches()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.Where(
                    e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.Read)));
            Assert.Equal(
                "Volvo",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.carFromSession.Make);
            Assert.Equal(this.carId, this.carFromSession.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadOnAnItemUpdatedInTheCurrentSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId, Active = false, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            await this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford");

            // When
            this.carFromSession = (await this.testHarness.DataStore.Read<Car>(car => car.Make == "Ford")).Single();
        }
    }
}