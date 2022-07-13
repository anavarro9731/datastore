namespace DataStore.Tests.Tests.Partitions
{
    using CircuitBoard;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenTryToUseTypeThatHasNoPartitionAttributes
    {
        [Fact]
        private async void ItShouldFailWithHierarchicalKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasNoPartitionAttributes));

            // When
           await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithNoKeyDefined()));
        }

        [Fact]
        private async void ItShouldFailWithSyntheticKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasNoPartitionAttributes), null, false);

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithNoKeyDefined()));
        }
    }
    
    public class WhenTryToUseTypeThatHasMoreThanOnePartitionAttribute
    {
        [Fact]
        private async void ItShouldFailWithHierarchicalKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasMoreThanOnePartitionAttribute));

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithTwoKeysDefined()));
        }

        [Fact]
        private async void ItShouldFailWithSyntheticKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasNoPartitionAttributes), null, false);

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithTwoKeysDefined()));
        }
    }
    
    public class WhenTryToUseTypeThatHasInvalidPropertyDefined
    {
        [Fact]
        private async void ItShouldFailWithHierarchicalKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasInvalidPropertyDefined));

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithBadKeyDefined()));
        }

        [Fact]
        private async void ItShouldFailWithSyntheticKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasNoPartitionAttributes), null, false);

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithBadKeyDefined()));
        }
    }
    
    public class WhenTryToQueryATypeThatRequiresOptions
    {
        [Fact]
        private async void ItShouldFailWithHierarchicalKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasInvalidPropertyDefined));

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithBadKeyDefined()));
        }

        [Fact]
        private async void ItShouldFailWithSyntheticKeys()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenTryToUseTypeThatHasNoPartitionAttributes), null, false);

            // When
            await Assert.ThrowsAsync<CircuitException>(async  () => await testHarness.DataStore.Create(new AggregateWithBadKeyDefined()));
        }
    }
}