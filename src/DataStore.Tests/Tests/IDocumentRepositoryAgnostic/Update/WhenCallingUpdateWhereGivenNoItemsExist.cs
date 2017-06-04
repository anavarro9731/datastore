namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereGivenNoItemsExist
    {
        public WhenCallingUpdateWhereGivenNoItemsExist()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWhereGivenNoItemsExist));

            //When
            result = testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "DoesNotExist", car => { }).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly IEnumerable<Car> result;
        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldNotExecuteAnyUpdate()
        {
            Assert.Null(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNoResults()
        {
            Assert.Empty(result);
        }
    }
}