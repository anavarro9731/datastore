using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateByIdGivenTheItemDoesNotExist
    {
        public WhenCallingUpdateByIdGivenTheItemDoesNotExist()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateByIdGivenTheItemDoesNotExist));

            var carId = Guid.NewGuid();

            //When
            result = testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Whatever").Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Car result;

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Null(result);
        }
    }
}