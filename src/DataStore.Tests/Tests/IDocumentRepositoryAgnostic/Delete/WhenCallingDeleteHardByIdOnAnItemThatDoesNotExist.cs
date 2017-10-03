namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist
    {
        private readonly Car result;

        public WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist));

            //When
            this.result = testHarness.DataStore.DeleteHardById<Car>(Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.result);
        }
    }
}