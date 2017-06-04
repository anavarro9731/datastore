namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingUpdateGivenTheItemDoesNotExist
    {
        public WhenCallingUpdateGivenTheItemDoesNotExist()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateGivenTheItemDoesNotExist));

            //When
            result = testHarness.DataStore.Update(new Car()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Car result;

        [Fact]
        public void ItShouldNotExecuteAnyUpdateOperation()
        {
            Assert.Equal(0, testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(result);
        }
    }
}