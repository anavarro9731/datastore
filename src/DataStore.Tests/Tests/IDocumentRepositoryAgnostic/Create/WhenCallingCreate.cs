namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreate
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        private Car result;

        public WhenCallingCreate()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreate));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            //When
            this.result = this.testHarness.DataStore.Create(newCar).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldFlushTheQueue()
        {
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }

        [Fact]
        public void ItShouldSetTheTimeStamps()
        {
            Assert.NotEqual(default(DateTime), this.result.Created);
            Assert.NotEqual(default(DateTime), this.result.Modified);
            Assert.NotEqual(default(double), this.result.CreatedAsMillisecondsEpochTime);
            Assert.NotEqual(default(double), this.result.ModifiedAsMillisecondsEpochTime);

        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Assert.Single(this.testHarness.DataStore.ReadActive<Car>().Result);
        }
    }
}