namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase
    {
        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase));

            var deletedCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            this.result = this.testHarness.DataStore.Update(deletedCar).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotExecuteAnyUpdateOperations()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.result);
        }
    }
}