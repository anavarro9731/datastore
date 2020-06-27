namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateGivenTheItemDoesNotExist
    {
        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotExecuteAnyUpdateOperation()
        {
            await Setup();
            Assert.Equal(0, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(this.result);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateGivenTheItemDoesNotExist));

            //When
            this.result = await this.testHarness.DataStore.Update(new Car());
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}