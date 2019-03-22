namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereGivenNoItemsExist
    {
        private readonly IEnumerable<Car> result;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateWhereGivenNoItemsExist()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWhereGivenNoItemsExist));

            //When
            this.result = this.testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "DoesNotExist", car => { }).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotExecuteAnyUpdate()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNoResults()
        {
            Assert.Empty(this.result);
        }
    }
}