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

    public class WhenCallingReadOnAnItemWhichHasBeenHardDeletedInTheCurrentSession
    {
        private Car carFromSession;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotReturnThatItem()
        {
            await Setup();

            //really helpful to debugging even if it doesn't matter here too much
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.Read)));
            Assert.Single(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Null(this.carFromSession);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadOnAnItemWhichHasBeenHardDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId, Active = false, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            await this.testHarness.DataStore.DeleteById<Car>(carId, o => o.Permanently());

            // When
            this.carFromSession = (await this.testHarness.DataStore.Read<Car>(car => car.id == carId)).SingleOrDefault();
        }
    }
}