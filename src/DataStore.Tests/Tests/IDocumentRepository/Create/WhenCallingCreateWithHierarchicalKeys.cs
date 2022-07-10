namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Tests.Models.PartitionKeyTestModels;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithHierarchicalKeys
    {
        [Fact]
        private async void AndAttributeWithSharedKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithSharedKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithSharedKey
            {
                id = newId
            };

            //when
            await Assert.ThrowsAsync<CircuitException>(async () => await testHarness.DataStore.Create(agg));
        }

        [Fact]
        private async void AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();

            var agg = new AggregateWithTypeIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeIdKey>(newId);
            Assert.NotNull(result);
            Assert.Null(result.PartitionKey);
            Assert.Equal(
                new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = $"TP:{typeof(AggregateWithTypeIdKey).FullName}", Key2 = $"ID:{newId}"
                },
                result.PartitionKeys);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantIdKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            var agg = new AggregateWithTypeTenantIdKey
            {
                id = newId, TenantId = tenantId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantIdKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(tenantId));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKey);
            Assert.Equal(
                new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = $"TP:{typeof(AggregateWithTypeTenantIdKey).FullName}", Key2 = $"TN:{tenantId}", Key3 = $"ID:{newId}"
                },
                result.PartitionKeys);
        }

        [Fact]
        private async void AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTenantTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var monthInterval = new MonthInterval(2022, 7);
            var tenantId = Guid.NewGuid();

            var agg = new AggregateWithTypeTenantTimePeriodKey
            {
                id = newId, TenantId = tenantId, TimePeriod = DateTime.UtcNow
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTenantTimePeriodKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(tenantId, monthInterval));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKey);
            Assert.Equal(
                new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = $"TP:{typeof(AggregateWithTypeTenantTimePeriodKey).FullName}", Key2 = $"TN:{tenantId}", Key3 = $"TM:{monthInterval}"
                },
                result.PartitionKeys);
        }

        [Fact]
        private async void AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys()
        {
            // Given
            var testHarness = TestHarness.Create(
                nameof(WhenCallingCreateWithHierarchicalKeys) + nameof(AndAttributeWithTypeTimePeriodKeyItShouldSaveTheRightKeys),
                useHierarchicalPartitionKey: true);

            var newId = Guid.NewGuid();
            var dayInterval = new DayInterval(2022, 7, 10); //* covers created date

            var agg = new AggregateWithTypeTimePeriodIdKey
            {
                id = newId
            };

            await testHarness.DataStore.Create(agg);
            await testHarness.DataStore.CommitChanges();

            //when
            var result = await testHarness.DataStore.WithoutEventReplay.ReadById<AggregateWithTypeTimePeriodIdKey>(
                             newId,
                             side => side.ProvidePartitionKeyValues(dayInterval));
            Assert.NotNull(result);
            Assert.Null(result.PartitionKey);
            Assert.Equal(
                new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = $"TP:{typeof(AggregateWithTypeTimePeriodIdKey).FullName}", Key2 = $"TM:{dayInterval}", Key3 = $"ID:{newId}"
                },
                result.PartitionKeys);
        }
    }
}