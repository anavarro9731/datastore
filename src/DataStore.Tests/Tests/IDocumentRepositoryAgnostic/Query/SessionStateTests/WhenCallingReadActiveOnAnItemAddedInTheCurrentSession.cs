namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveOnAnItemAddedInTheCurrentSession
    {
        private readonly Car newCarFromSession;

        private readonly ITestHarness testHarness;

        public WhenCallingReadActiveOnAnItemAddedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            var newCarId = Guid.NewGuid();
            this.testHarness.DataStore.Create(
                new Car
                {
                    id = newCarId,
                    Active = true,
                    Make = "Ford"
                }).Wait();

            this.newCarFromSession = this.testHarness.DataStore.ReadActive<Car>(c => c.id == Guid.NewGuid()).Result.Single();
        }

        [Fact]
        public void ItShouldNotHaveAddedThatItemToTheDatabaseYet()
        {
            Assert.Single(this.testHarness.QueryDatabase<Car>());
            Assert.Equal(2, this.testHarness.DataStore.ReadActive<Car>().Result.Count());
        }

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.NotNull(this.newCarFromSession);
        }
    }
}