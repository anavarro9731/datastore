namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    #region

    using System;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_ImmutableTenantId_Id(nameof(TenantId))]
    public class AggregateWithTypeTenantIdKey : Aggregate
    {
        public Guid TenantId { get; set; }
        public int TestValue { get; set; }
    }
}