namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingCommitMultipleTimesAfterACreateOperation
    {
        public WhenCallingCommitMultipleTimesAfterACreateOperation()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitMultipleTimesAfterACreateOperation));

            newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            car = testHarness.DataStore.Create(newCar).Result;

            //When
            testHarness.DataStore.CommitChanges().Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;
        private readonly Car car;

        [Fact]
        public void ItShouldPersistChangesToTheDatabaseOnlyOnce()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(testHarness.QueryDatabase<Car>().Single().id == newCarId);
        }

        [Fact]
        public void ItShouldReturnTheNewCarFromTheDatabaseWithSomeUpdateProperties()
        {
            Assert.NotNull(car);
            Assert.Equal(car.schema, typeof(Car).FullName);
            Assert.False(car.ReadOnly);
            Assert.NotNull(car.ScopeReferences);
        }

        [Fact]
        public void ThereShouldBeNoOperationsInTheQueue()
        {
            Assert.Null(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }
    }
}