namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    #region

    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_Id]
    public class AggregateWithTypeIdKey : Aggregate
    {
        public int TestValue { get; set; }
    }
}