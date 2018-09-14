namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithAPredicateOnAnItemAddedInTheCurrentSession
    {
        private readonly ITestHarness testHarness;

        private readonly Guid fordId;

        public WhenCallingReadActiveWithAPredicateOnAnItemAddedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            this.testHarness.AddToDatabase(new Car
            {
                id = Guid.NewGuid(),
                Active = true,
                Make = "Lambo"
            });

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = Guid.NewGuid(),
                    Active = true,
                    Make = "Volvo"
                }).Wait();

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = Guid.NewGuid(),
                    Active = true,
                    Make = "Mazda"
                }).Wait();

            this.fordId = Guid.NewGuid();

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId,
                    Active = true,
                    Make = "Ford"
                }).Wait();
        }

        [Fact]
        public void ItShouldNotHaveAddedTheFordToTheDatabaseYet()
        {
            Assert.ThrowsAny<Exception>(() => this.testHarness.QueryDatabase<Car>().Single(x => x.id == this.fordId));
        }

        [Fact]
        public void ItShouldNotReturnThatItemIfThePredicateDoesntMatch()
        {
            var newCarFromSession = this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "NonExistantMake").Result.SingleOrDefault();
            Assert.Null(newCarFromSession);
        }

        [Fact]
        public void ItShouldReturnThatItemIfThePredicateMatches()
        {
            var newCarFromSession = this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford").Result.SingleOrDefault();
            Assert.NotNull(newCarFromSession);
            Assert.Equal(this.fordId, newCarFromSession.id);            
        }
    }
}