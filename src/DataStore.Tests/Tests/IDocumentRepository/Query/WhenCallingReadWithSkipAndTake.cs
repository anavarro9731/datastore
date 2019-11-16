namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithSkipAndTake
    {
        private IEnumerable<Car> carsFromDatabase;

        private Guid fourthCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        [Fact]
        public async void ItShouldReturnTheLastTwoVolvos()
        {
            await Setup();
            Assert.Equal(2, this.carsFromDatabase.Count());
            Assert.Equal(this.thirdCarId, this.carsFromDatabase.First().id);
            Assert.Equal(this.fourthCarId, this.carsFromDatabase.Last().id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithSkipAndTake));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Ford"
            };

            this.thirdCarId = Guid.NewGuid();
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId,
                Active = true,
                Make = "Volvo"
            };

            this.fourthCarId = Guid.NewGuid();
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId,
                Active = true,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);
            this.testHarness.AddToDatabase(thirdExistingCar);
            this.testHarness.AddToDatabase(fourthExistingCar);

            // When
            var firstContinuationToken = new ContinuationToken();

            await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car, WithoutReplayOptions<Car>>(
                car => car.Make == "Volvo",
                o => o.Take(1, ref firstContinuationToken));

            var secondContinuationToken = new ContinuationToken();

            this.carsFromDatabase = await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car, WithoutReplayOptions<Car>>(
                                        car => car.Make == "Volvo",
                                        o => o.ContinueFrom(firstContinuationToken).Take(2, ref secondContinuationToken));
        }
    }
}