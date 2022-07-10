namespace DataStore.Tests.Tests.Repo
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Models;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingGetItemAsyncDynamically
    {
        private Car newCar;

        private ITestHarness testHarness;

                
        [Fact]
        public async void WithHierarchicalKeysItShouldBeAbleToQueryTheItem()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingGetItemAsyncDynamically), useHierarchicalPartitionKey:true);

            this.newCar = new Car
            {
                id = Guid.NewGuid(), Make = "Volvo"
            };

            this.newCar.PartitionKeys = PartitionKeyHelpers.GetKeysForNewModel(this.newCar, useHierarchicalPartitionKeys: true).PartitionKeys;

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(this.newCar, "Test");
            
            //Then
            Assert.True(await this.testHarness.DataStore.DocumentRepository.Exists(this.newCar));
        }
        
        
        [Fact]
        public async void WithSyntheticKeysItShouldBeAbleToQueryTheItem()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingGetItemAsyncDynamically), useHierarchicalPartitionKey:false);

            this.newCar = new Car
            {
                id = Guid.NewGuid(), Make = "Volvo"
            };

            this.newCar.PartitionKey = PartitionKeyHelpers.GetKeysForNewModel(this.newCar, useHierarchicalPartitionKeys: false).PartitionKey;

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(this.newCar, "Test");
            
            //Then
            Assert.True(await this.testHarness.DataStore.DocumentRepository.Exists(this.newCar));
        }
        
    }
}