namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithAPredicateOnAnItemAddedInTheCurrentSession
    {
        private Guid fordId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotHaveAddedTheFordToTheDatabaseYet()
        {
            await Setup();
            var result = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(x => x.id == this.fordId));

            Assert.Empty(result);
        }

        [Fact]
        public async void ItShouldNotReturnThatItemIfThePredicateDoesntMatch()
        {
            await Setup();
            var newCarFromSession = (await this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "NonExistantMake")).SingleOrDefault();
            Assert.Null(newCarFromSession);
        }

        [Fact]
        public async void ItShouldReturnThatItemIfThePredicateMatches()
        {
            await Setup();
            var newCarFromSession = (await this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford")).SingleOrDefault();
            Assert.NotNull(newCarFromSession);
            Assert.Equal(this.fordId, newCarFromSession.id);
        }

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveWithAPredicateOnAnItemAddedInTheCurrentSession));

            this.testHarness.AddItemDirectlyToUnderlyingDb(
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

            this.fordId = Guid.Parse("1677696a-01e5-4f71-ada4-3ea26b25f95e");

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId, Active = true, Make = "Ford"
                });
        }
    }
}