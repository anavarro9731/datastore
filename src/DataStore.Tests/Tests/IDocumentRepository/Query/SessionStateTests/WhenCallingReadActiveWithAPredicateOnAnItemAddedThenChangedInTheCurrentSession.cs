namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithAPredicateOnAnItemAddedThenChangedInTheCurrentSession
    {
        private Guid fordId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotHaveAddedTheFordToTheDatabaseYet()
        {
            await Setup();
            var result = this.testHarness.QueryDatabase<Car>(cars => cars.Where(x => x.id == this.fordId));
            Assert.Empty(result);
        }

        [Fact]
        public async void ItShouldNotReturnThatItemIfThePredicateDoesntMatch()
        {
            await Setup();
            var newCarFromSession = (await this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford")).SingleOrDefault();
            Assert.Null(newCarFromSession);
        }

        [Fact]
        public async void ItShouldReturnThatItemIfThePredicateMatches()
        {
            await Setup();
            var newCarFromSession = (await this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford2")).SingleOrDefault();
            Assert.NotNull(newCarFromSession);
            Assert.Equal(this.fordId, newCarFromSession.id);
        }

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveWithAPredicateOnAnItemAddedThenChangedInTheCurrentSession));

            this.testHarness.AddToDatabase(
                new Car
                {
                    id = Guid.NewGuid(), Active = true, Make = "Lambo"
                });

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = Guid.NewGuid(), Active = true, Make = "Volvo"
                });

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = Guid.NewGuid(), Active = true, Make = "Mazda"
                });

            this.fordId = Guid.NewGuid();

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId, Active = true, Make = "Ford"
                });

            await this.testHarness.DataStore.UpdateById<Car>(this.fordId, c => c.Make = "Ford2");
        }
    }
}