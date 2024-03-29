namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadActiveWithSkipAndTake
    {
        private IEnumerable<Car> carsFromDatabase;

        private Guid fourthCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        [Fact]
        public async void ItShouldReturnTheLastVolvo()
        {
            await Setup();
            Assert.Single(this.carsFromDatabase);
            Assert.Equal(this.fourthCarId, this.carsFromDatabase.Single().id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveWithSkipAndTake));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId, Make = "Ford"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId, Active = false, Make = "Volvo"
            };

            this.thirdCarId = Guid.NewGuid();
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId, Active = true, Make = "Volvo"
            };

            this.fourthCarId = Guid.NewGuid();
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId, Active = true, Make = "Volvo"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(inactiveExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(thirdExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(fourthExistingCar);

            // When
            var firstContinuationToken = new ContinuationToken();

            await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car>(
                car => car.Make == "Volvo",
                o => o.Take(1, ref firstContinuationToken));

            var secondContinuationToken = new ContinuationToken();
            this.carsFromDatabase = await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car>(
                                        car => car.Make == "Volvo",
                                        o => o.ContinueFrom(firstContinuationToken).Take(2, ref secondContinuationToken));
        }
    }
}