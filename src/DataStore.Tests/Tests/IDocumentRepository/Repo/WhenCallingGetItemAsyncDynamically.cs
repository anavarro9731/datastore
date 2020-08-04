namespace DataStore.Tests.Tests.IDocumentRepository.Repo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models;
    using global::DataStore.Models.Messages;
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
            Assert.NotNull(this.testHarness.DataStore.DocumentRepository.GetItemAsync(this.newCar));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingAddAsyncDynamically));

            this.newCar = new Car
            { 
                Make = "Volvo"
            };

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(newCar, "Test");
        }
    }
}