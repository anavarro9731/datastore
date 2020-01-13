namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithoutCommitting
    {
        private  Car result;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithoutCommitting));

            var newCar = new Car
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            this.result = await this.testHarness.DataStore.Create(newCar);
        }

        [Fact]
        public async void ItShouldNotWriteToTheDatabase()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public async void ItShouldSetTheEtagsCorrectly()
        {
            await Setup();
            Assert.Equal("waiting to be committed", this.result.Etag);
        }

        [Fact]
        public async void ItShouldReflectTheChangeInFutureQueriesFromTheSameSession()
        {
            await Setup();
            Assert.Single(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.True(this.result.Active);
        }
    }
}