namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    #region

    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Shared]
    public class AggregateWithSharedKey : Aggregate
    {
        public int TestValue { get; set; }
    }
}