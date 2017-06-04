namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist
    {
        public WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist));

            //When
            result = testHarness.DataStore.DeleteHardById<Car>(Guid.NewGuid()).Result;
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