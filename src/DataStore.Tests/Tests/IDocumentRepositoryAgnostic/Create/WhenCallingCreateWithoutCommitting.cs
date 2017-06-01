using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    public class WhenCallingCreateWithoutCommitting
    {
        public WhenCallingCreateWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingCreateWithoutCommitting));

            var newCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            result = testHarness.DataStore.Create(newCar).Result;
        }

        private readonly Car result;
        private readonly ITestHarness testHarness;


        [Fact]
        public void ItShouldOnlyMakeTheChangesInSession()
        {
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.Equal(0, testHarness.QueryDatabase<Car>().Count());
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
            Assert.True(result.Active);
        }
    }
}