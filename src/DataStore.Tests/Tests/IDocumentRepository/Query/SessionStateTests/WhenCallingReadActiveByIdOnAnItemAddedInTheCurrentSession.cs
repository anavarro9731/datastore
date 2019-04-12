namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession
    {
        private Car newCarFromSession;

        private ITestHarness testHarness;

        private Guid fordId;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            var volvoId = Guid.NewGuid();
            
            await this.testHarness.DataStore.Create(new Car
                {
                    id = volvoId,
                    Active = true,
                    Make = "Volvo"
                });

            this.fordId = Guid.NewGuid();

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId,
                    Active = true,
                    Make = "Ford"
                });

            this.newCarFromSession = await this.testHarness.DataStore.ReadActiveById<Car>(this.fordId);
        }

        [Fact]
        public async void ItShouldNotHaveAddedAnythingToTheDatabaseYet()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Equal(2, (await this.testHarness.DataStore.ReadActive<Car>()).Count());
        }

        [Fact]
        public async void ItShouldReturnThatItem()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregateQueriedByIdOperation));
            Assert.NotNull(this.newCarFromSession);
            Assert.Equal(this.fordId, this.newCarFromSession.id);
        }
    }
}