using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase
    {
        public WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateOnAnItemThatNoLongerExistsInTheDatabase));

            var deletedCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            result = testHarness.DataStore.Update(deletedCar).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Car result;

        [Fact]
        public void ItShouldNotExecuteAnyUpdateOperations()
        {
            Assert.Null(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(result);
        }
    }
}