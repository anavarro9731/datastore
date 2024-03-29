namespace DataStore.Tests.Tests.Delete
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteHard
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is HardDeleteOperation<Car> && e.MethodCalled == nameof(DataStore.Delete)));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldReturnTheItemDeleted()
        {
            await Setup();
            Assert.Equal(this.carId, this.result.id);
        }

        [Fact]
        public async void ItShouldSetTheEtagsCorrectly()
        {
            await Setup();
            Assert.Equal("item was deleted", this.result.Etag);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHard), DataStoreOptions.Create());

            this.carId = Guid.NewGuid();
            var car = await this.testHarness.DataStore.Create(
                          new Car
                          {
                              id = this.carId, Make = "Volvo"
                          });

            await this.testHarness.DataStore.CommitChanges();

            //When
            this.result = await this.testHarness.DataStore.Delete(car, o => o.Permanently());
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}