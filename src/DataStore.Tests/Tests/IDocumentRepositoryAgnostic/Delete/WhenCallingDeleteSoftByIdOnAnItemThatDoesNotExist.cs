namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist
    {
        public WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist));

            //When
            result = testHarness.DataStore.DeleteSoftById<Car>(Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly Car result;

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(result);
        }
    }
}