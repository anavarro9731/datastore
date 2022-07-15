namespace DataStore.Tests.Tests.RuntimeTyping
{
    #region

    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteAsyncDynamically
    {
        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteAsyncDynamically), DataStoreOptions.Create());

            this.carId = Guid.NewGuid();
            var car = await this.testHarness.DataStore.Create(
                          new Car
                          {
                              id = this.carId, Make = "Volvo"
                          });

            await this.testHarness.DataStore.CommitChanges();

            //When
            await this.testHarness.DataStore.DocumentRepository.DeleteAsync(car);
        }
    }
}