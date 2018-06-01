namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithoutCommitting
    {
        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingCreateWithoutCommitting()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateWithoutCommitting));

            var newCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            this.result = this.testHarness.DataStore.Create(newCar).Result;
        }

        [Fact]
        public void ItShouldNotWriteToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public void ItShouldReflectTheChangeInFutureQueriesFromTheSameSession()
        {
            Assert.Single(this.testHarness.DataStore.ReadActive<Car>().Result);
            Assert.True(this.result.Active);
        }
    }
}