namespace DataStore.Tests.Tests.IDocumentRepository.Repo
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingGetItemAsyncDynamically
    {
        private Car newCar;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldBeAbleToQueryTheItem()
        {
            await Setup();
            Assert.True(await this.testHarness.DataStore.DocumentRepository.Exists(this.newCar));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingAddAsyncDynamically));

            this.newCar = new Car
            {
                id = Guid.NewGuid(), Make = "Volvo"
            };

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(this.newCar, "Test");
        }
    }
}