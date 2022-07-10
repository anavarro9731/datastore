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

    public class WhenCallingUpdateWhereWithoutCommitting
    {
        private Guid carId;

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

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWhereWithoutCommitting));

            this.carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            //When
            await this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford");
        }
    }
}