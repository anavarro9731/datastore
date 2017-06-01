using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    public class WhenChangingTheItemsReturnedFromCreate
    {
        public WhenChangingTheItemsReturnedFromCreate()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemsReturnedFromCreate));

            newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            var result = testHarness.DataStore.Create(newCar).Result;

            //When
            result.id = Guid.NewGuid();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Count());
            Assert.True(testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(testHarness.QueryDatabase<Car>().Single().id == newCarId);
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
        }
    }
}