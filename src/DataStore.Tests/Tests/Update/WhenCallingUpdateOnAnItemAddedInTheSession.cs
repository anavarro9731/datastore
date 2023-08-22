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

    public class WhenCallingUpdateOnAnItemAddedInThisSession
    {
        private Guid carId;

        private string firstEtag;

        private string secondEtag;

        private ITestHarness testHarness;

        [Fact]
        public async void  ItShouldCallCreateOnlyOnce()
        {
            await Setup();
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is CreateOperation<Car>));
        }

        [Fact]
        public async void ItShouldPersistTheLastChangeToTheDatabase()
        {
            await Setup();
            Assert.Equal("Toyota", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
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
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateOnAnItemAddedInThisSession));

            this.carId = Guid.NewGuid();
            
            var newCar = new Car
            {
                id = this.carId, 
                Make = "Volvo"
            };
            
            var create = await this.testHarness.DataStore.Create(newCar);

            // When
            var update = await this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Toyota");
            
            await this.testHarness.DataStore.CommitChanges();
            
            this.firstEtag = create.Etag;
            this.secondEtag = update.Etag;
        }
    }
}