namespace DataStore.Tests.Tests.Update
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

    public class WhenCallingUpdateTwiceInOneSession
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
            Assert.Equal("Toyota", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
            Assert.Equal(2001, (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Year);
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
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateTwiceInOneSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId, Make = "Volvo", Year = 2000
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(existingCar);

            //When
            var update1 = await this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Toyota");
            var update2 = await this.testHarness.DataStore.UpdateById<Car>(this.carId, c =>
                {
                    c.Year = 2001;
                });
            await this.testHarness.DataStore.CommitChanges();
            this.firstEtag = update1.Etag;
            this.secondEtag = update2.Etag;
        }
    }
}
