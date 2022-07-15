namespace DataStore.Tests.Tests.RuntimeTyping
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Models;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

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

            var key = PartitionKeyHelpers.GetKeysForNewModel(this.newCar, useHierarchicalPartitionKeys: false);
            this.newCar.PartitionKeys = key;
            this.newCar.PartitionKey = key.ToSyntheticKeyString();

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

            var key = PartitionKeyHelpers.GetKeysForNewModel(this.newCar, useHierarchicalPartitionKeys: false);
            this.newCar.PartitionKeys = key;
            this.newCar.PartitionKey = key.ToSyntheticKeyString();

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(this.newCar, "Test");
            
            //Then
            Assert.True(await this.testHarness.DataStore.DocumentRepository.Exists(this.newCar));
        }
        
    }
}