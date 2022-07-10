namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    [PartitionKey__Type_ImmutableTenantId_TimePeriod(nameof(TenantId), nameof(TimePeriod), PartitionKeyTimeIntervalEnum.Month)]
    public class AggregateWithTypeTenantTimePeriodKey : Aggregate
    {
        public Guid? TenantId { get; set; }

        public DateTime? TimePeriod { get; set; }
    }
}