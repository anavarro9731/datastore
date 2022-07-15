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

    public class WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession
    {
        private Car carFromSession;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotReturnThatItem()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.Where(
                    e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.ReadActive)));
            Assert.Single(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Null(this.carFromSession);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId, Active = false, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            await this.testHarness.DataStore.DeleteById<Car>(carId);

            // When
            this.carFromSession = (await this.testHarness.DataStore.ReadActive<Car>(car => car.id == carId)).SingleOrDefault();
        }
    }
}