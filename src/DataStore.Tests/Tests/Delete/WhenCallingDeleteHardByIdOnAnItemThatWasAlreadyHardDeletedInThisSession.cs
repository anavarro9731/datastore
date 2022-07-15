namespace DataStore.Tests.Tests.Delete
{
    #region

    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession
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
        public async void ItShouldCollapseTheDeletes()
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

            await this.testHarness.DataStore.DeleteById<Car>(this.newCarId, o => o.Permanently());

            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.newCarId, o => o.Permanently());
        }
    }
}