namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereGivenNoItemsExist
    {
        private IEnumerable<Car> result;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWhereGivenNoItemsExist));

            //When
            this.result = await this.testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "DoesNotExist", car => { });
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldNotExecuteAnyUpdate()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldReturnNoResults()
        {
            await Setup();
            Assert.Empty(this.result);
        }
    }
}