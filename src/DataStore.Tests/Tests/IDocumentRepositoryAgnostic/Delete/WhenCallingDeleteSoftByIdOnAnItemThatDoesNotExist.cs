namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist
    {
        private readonly Car result;

        public WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist));

            //When
            this.result = testHarness.DataStore.DeleteSoftById<Car>(Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.result);
        }
    }
}