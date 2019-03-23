namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession
    {
        private readonly Car newCarFromSession;

        private readonly ITestHarness testHarness;

        private readonly Guid fordId;

        public WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            var volvoId = Guid.NewGuid();
            
            this.testHarness.DataStore.Create(new Car
                {
                    id = volvoId,
                    Active = true,
                    Make = "Volvo"
                }).Wait();

            this.fordId = Guid.NewGuid();

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId,
                    Active = true,
                    Make = "Ford"
                }).Wait();

            this.newCarFromSession = this.testHarness.DataStore.ReadActiveById<Car>(this.fordId).Result;
        }

        [Fact]
        public void ItShouldNotHaveAddedAnythingToTheDatabaseYet()
        {
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Equal(2, this.testHarness.DataStore.ReadActive<Car>().Result.Count());
        }

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregateQueriedByIdOperation));
            Assert.NotNull(this.newCarFromSession);
            Assert.Equal(this.fordId, this.newCarFromSession.id);
        }
    }
}