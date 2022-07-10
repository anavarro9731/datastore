namespace DataStore.Tests.Tests.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingSoftDeleteOnAnItemUpdatedInThisSession
    {
        private Guid carId;

        private string firstEtag;

        private string secondEtag;

        private ITestHarness testHarness;

        [Fact]
        public async void  ItShouldCallUpdateOnlyOnce()
        {
            await Setup();
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldPersistBothChangesToTheDatabase()
        {
            await Setup();
            Assert.Equal("Toyota", (await this.testHarness.DataStore.ReadById<Car>(this.carId)).Make);
            Assert.False((await this.testHarness.DataStore.ReadById<Car>(this.carId)).Active);
        }

        [Fact]
        public async void TheFirstAndSecondEtagShouldBeTheSameBecauseTheyAreTheSameUpdate()
        {
            await Setup();
            Assert.NotNull(this.firstEtag);
            Assert.Equal(this.firstEtag, this.secondEtag);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingSoftDeleteOnAnItemUpdatedInThisSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId, Make = "Volvo"
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            //When
            var update1 = await this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Toyota");
            var update2 = await this.testHarness.DataStore.DeleteById<Car>(this.carId);
            await this.testHarness.DataStore.CommitChanges();
            this.firstEtag = update1.Etag;
            this.secondEtag = update2.Etag;
        }
    }
}
