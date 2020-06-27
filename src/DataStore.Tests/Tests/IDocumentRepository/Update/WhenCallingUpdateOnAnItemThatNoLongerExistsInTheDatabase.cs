namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase
    {
        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotExecuteAnyUpdateOperations()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
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
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase));

            var deletedCar = new Car
            {
                id = Guid.NewGuid(), Make = "Volvo"
            };

            //When
            this.result = await this.testHarness.DataStore.Update(deletedCar);
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}