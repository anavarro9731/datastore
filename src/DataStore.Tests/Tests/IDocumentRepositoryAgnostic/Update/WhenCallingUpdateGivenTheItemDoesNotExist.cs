namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateGivenTheItemDoesNotExist
    {
        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateGivenTheItemDoesNotExist()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateGivenTheItemDoesNotExist));

            //When
            this.result = this.testHarness.DataStore.Update(new Car()).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotExecuteAnyUpdateOperation()
        {
            Assert.Equal(0, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.result);
        }
    }
}