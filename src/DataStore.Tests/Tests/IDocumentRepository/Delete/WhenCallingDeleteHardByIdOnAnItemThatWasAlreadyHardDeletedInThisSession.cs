namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession
    {
        private Car result;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();

            Assert.Null(this.result);

        }

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession));

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.newCarId = Guid.NewGuid(), Make = "Ford"
                });

            await this.testHarness.DataStore.CommitChanges();

            await this.testHarness.DataStore.DeleteHardById<Car>(this.newCarId);

            this.result = await this.testHarness.DataStore.DeleteHardById<Car>(this.newCarId);
        }
    }
}