namespace DataStore.Models
{
    #region

    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_Id]
    public class AggregateHistoryItem<TAggregate> : Aggregate where TAggregate : IAggregate
    {
        public TAggregate AggregateVersion { get; set; }

        public string AssemblyQualifiedTypeName { get; set; }
    }
}