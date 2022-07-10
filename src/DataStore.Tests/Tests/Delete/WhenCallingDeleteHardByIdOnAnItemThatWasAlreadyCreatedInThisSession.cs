namespace DataStore.Tests.Tests.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyCreatedInThisSession
    {
        private Guid newCarId;

        private Car result;

        private ITestHarness testHarness;

        private Guid volvoId;

        [Fact]
        public async void ItShouldReturnTheItem() 
        {
            await Setup();

            Assert.NotNull(this.result);
        }
        
        [Fact]
        public async void ItShouldNotQueueTheCreateOperationForTheObjectThatWasDeleted()
        {
            await Setup();

            Assert.Equal(0, this.testHarness.DataStore.QueuedOperations.Count(o => o.AggregateId == this.newCarId));
        }
        
        [Fact]
        public async void ItShouldQueueTheCreateOperationForTheObjectThatWasNotDeleted()
        {
            await Setup();

            Assert.Equal(1, this.testHarness.DataStore.QueuedOperations.Count(o => o.AggregateId == this.volvoId));
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
            
            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.volvoId = Guid.NewGuid(), Make = "Volvo"
                });
            
            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.newCarId, o => o.Permanently());
        }
    }
}