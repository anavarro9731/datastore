namespace DataStore.Tests.Tests.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateByIdWithoutCommitting
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldOnlyMakeTheChangesInSession()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(
                "Volvo",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }

        [Fact]
        public async void ItShouldSetTheEtagsCorrectly()
        {
            await Setup();
            Assert.Equal("waiting to be committed", this.result.Etag);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateByIdWithoutCommitting));

            this.carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            //When
            this.result = await this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford");
        }
    }
}