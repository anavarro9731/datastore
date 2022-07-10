namespace DataStore.Tests.Tests.SessionState
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession
    {
        private Guid fordId;

        private Car newCarFromSession;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotHaveAddedAnythingToTheDatabaseYet()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Equal(2, (await this.testHarness.DataStore.ReadActive<Car>()).Count());
        }

        [Fact]
        public async void ItShouldReturnThatItem()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is IDataStoreReadByIdOperation));
            Assert.NotNull(this.newCarFromSession);
            Assert.Equal(this.fordId, this.newCarFromSession.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            var volvoId = Guid.NewGuid();

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = volvoId, Active = true, Make = "Volvo"
                });

            this.fordId = Guid.NewGuid();

            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.fordId, Active = true, Make = "Ford"
                });

            this.newCarFromSession = await this.testHarness.DataStore.ReadActiveById<Car>(this.fordId);
        }
    }
}