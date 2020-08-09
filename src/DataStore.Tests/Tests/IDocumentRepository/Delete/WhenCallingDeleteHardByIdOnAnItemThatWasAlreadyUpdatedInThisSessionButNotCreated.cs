namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyUpdatedInThisSessionButNotCreated
    {
        private Guid newCarId;

        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnTheItem() 
        {
            await Setup();

            Assert.NotNull(this.result);
        }
        
        [Fact]
        public async void ItShouldRemoveTheUpdate()
        {
            await Setup();

            Assert.Empty(this.testHarness.DataStore.QueuedOperations.Where(x => x is QueuedUpdateOperation<Car>));
        }
        
        [Fact]
        public async void ItShouldNotQueueADeleteToTheDatabase()
        {
            await Setup();

            Assert.Equal(1, this.testHarness.DataStore.QueuedOperations.Count);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession));

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.newCarId = Guid.NewGuid(), Make = "Ford"
                });

            await this.testHarness.DataStore.CommitChanges();
            
            this.result = await this.testHarness.DataStore.UpdateById<Car>(this.newCarId, car => car.Make = "Tucker");
            
            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.newCarId, o => o.Permanently());

        }
    }
}