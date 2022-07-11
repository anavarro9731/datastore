namespace DataStore.Tests.Tests.Partitions.Update
{
    using System;
    using CircuitBoard;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdate
    {
        [Fact]
        private async void WithHierarchicalKeysItShouldNotAllowYouToUpdateTheKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WithSyntheticKeysItShouldNotAllowYouToUpdateTheKeys), useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.UpdateById<AggregateWithTypeIdKey>(newId, a => a.PartitionKey = "new key"));
            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.UpdateById<AggregateWithTypeIdKey>(newId, a => a.PartitionKeys = new Aggregate.HierarchicalPartitionKey()
            {
                Key1 = "new key"
            }));
        }

        [Fact]
        private async void WithSyntheticKeysItShouldNotAllowYouToUpdateTheKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WithSyntheticKeysItShouldNotAllowYouToUpdateTheKeys), useHierarchicalPartitionKey: false);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            await Assert.ThrowsAsync<CircuitException>(
                async () => await testHarness.DataStore.UpdateById<AggregateWithTypeIdKey>(newId, a => a.PartitionKey = "new key"));
            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.UpdateById<AggregateWithTypeIdKey>(newId, a => a.PartitionKeys = new Aggregate.HierarchicalPartitionKey()
            {
                Key1 = "new key"
            }));
        }
    }
}