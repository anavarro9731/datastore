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

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyCreatedInThisSession
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
        public async void ItShouldRemoveTheCreate()
        {
            await Setup();

            Assert.Empty(this.testHarness.DataStore.QueuedOperations.Where(x => x is QueuedCreateOperation<Car>));
        }
        
        
        [Fact]
        public async void ItShouldNotQueueAnyDatabaseOperations()
        {
            await Setup();

            Assert.Equal(0, this.testHarness.DataStore.QueuedOperations.Count);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyCreatedInThisSession));

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.newCarId = Guid.NewGuid(), Make = "Ford"
                });
            
            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.newCarId, o => o.Permanently());
        }
    }
}