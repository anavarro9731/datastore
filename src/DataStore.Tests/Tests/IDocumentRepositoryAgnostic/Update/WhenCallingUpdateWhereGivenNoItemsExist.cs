using System.Collections.Generic;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateWhereGivenNoItemsExist
    {
        public WhenCallingUpdateWhereGivenNoItemsExist()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(ItShouldReturnNoResults));

            //When
            result = testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "DoesNotExist", car => { }).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly IEnumerable<Car> result;
        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldReturnNoResults()
        {
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Empty(result);
        }
    }
}