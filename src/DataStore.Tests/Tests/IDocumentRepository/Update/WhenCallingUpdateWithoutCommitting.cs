namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWithoutCommitting
    {
        private Guid carId;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWithoutCommitting));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);
            existingCarFromDb.Make = "Ford";
            await this.testHarness.DataStore.Update(existingCarFromDb);
        }

        [Fact]
        public async void ItShouldOnlyMakeTheChangesInSession()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford",(await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }
    }
}