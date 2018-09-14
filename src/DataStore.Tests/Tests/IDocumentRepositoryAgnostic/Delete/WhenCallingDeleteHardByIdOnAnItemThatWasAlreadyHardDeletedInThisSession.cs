namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession
    {

        private readonly Exception e;
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardByIdOnAnItemThatWasAlreadyHardDeletedInThisSession));

            this.testHarness.DataStore.Create(
                new Car()
                {
                    id = this.newCarId = Guid.Empty,
                    Make = "Ford"
                }).Wait();

            this.testHarness.DataStore.DeleteHardById<Car>(this.newCarId).Wait();
        }

        [Fact]
        public void ItShouldErrorWhenYouDeleteTheSecondTime()
        {
            Assert.ThrowsAny<Exception>(() => testHarness.DataStore.DeleteHardById<Car>(this.newCarId).Wait());
        }
    }
}