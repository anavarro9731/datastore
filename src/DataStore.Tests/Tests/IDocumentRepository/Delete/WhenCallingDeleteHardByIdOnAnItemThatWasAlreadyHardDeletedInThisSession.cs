namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession
    {
        private Exception e;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldErrorWhenYouDeleteTheSecondTime()
        {
            await Setup();

            Assert.Contains("c53bef0f-a462-49cc-8d73-04cdbb3ea81c", this.e.Message);
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

            await this.testHarness.DataStore.DeleteHardById<Car>(this.newCarId);

            this.e = await Assert.ThrowsAnyAsync<Exception>(async ()=> await this.testHarness.DataStore.DeleteHardById<Car>(this.newCarId));
        }
    }
}