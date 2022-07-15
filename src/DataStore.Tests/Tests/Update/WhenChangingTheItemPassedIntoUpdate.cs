namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenChangingTheItemPassedIntoUpdate
    {
        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.Equal(
                "Ford",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemPassedIntoUpdate));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            //read from db to pickup changes to properties made by datastore oncreate
            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);
            existingCarFromDb.Make = "Ford";

            await this.testHarness.DataStore.Update(existingCarFromDb);

            //change the id before committing, if not cloned this would cause the item not to be found
            existingCarFromDb.id = Guid.NewGuid();

            //When
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}