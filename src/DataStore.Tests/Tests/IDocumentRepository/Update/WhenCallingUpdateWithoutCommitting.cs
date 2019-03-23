namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWithoutCommitting
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateWithoutCommitting()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWithoutCommitting));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                Id = this.carId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result;
            existingCarFromDb.Make = "Ford";
            this.testHarness.DataStore.Update(existingCarFromDb).Wait();
        }

        [Fact]
        public void ItShouldOnlyMakeTheChangesInSession()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.Id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}