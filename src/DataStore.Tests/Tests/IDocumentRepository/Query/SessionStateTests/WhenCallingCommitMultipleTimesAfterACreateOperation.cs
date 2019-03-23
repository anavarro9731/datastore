namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCommitMultipleTimesAfterACreateOperation
    {
        private readonly Car car;

        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingCommitMultipleTimesAfterACreateOperation()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCommitMultipleTimesAfterACreateOperation));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            this.car = this.testHarness.DataStore.Create(newCar).Result;

            //When
            this.testHarness.DataStore.CommitChanges().Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabaseOnlyOnce()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
        }

        [Fact]
        public void ItShouldReturnTheNewCarFromTheDatabaseWithSomeUpdateProperties()
        {
            Assert.NotNull(this.car);
            Assert.Equal(this.car.Schema, typeof(Car).FullName);
            Assert.False(this.car.ReadOnly);
            Assert.NotNull(this.car.ScopeReferences);
        }

        [Fact]
        public void ThereShouldBeNoOperationsInTheQueue()
        {
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }
    }
}