namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingCreate
    {
        public WhenCallingCreate()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreate));

            newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            //When
            testHarness.DataStore.Create(newCar).Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldFlushTheQueue()
        {
            Assert.Null(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(testHarness.QueryDatabase<Car>().Single().id == newCarId);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
        }
    }

    public class WhenCallingCreateWithTheReadOnlyFlagSetToTrue
    {
        public WhenCallingCreateWithTheReadOnlyFlagSetToTrue()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateWithTheReadOnlyFlagSetToTrue));

            newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            //When
            testHarness.DataStore.Create(newCar, true).Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.True(testHarness.QueryDatabase<Car>().Single().ReadOnly);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Assert.Equal(true, testHarness.DataStore.ReadActiveById<Car>(newCarId).Result.ReadOnly);
        }
    }
}