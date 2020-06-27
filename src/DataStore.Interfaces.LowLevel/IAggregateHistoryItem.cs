namespace DataStore.Interfaces.LowLevel
{
    public class AggregateHistoryItem<TAggregate> : Aggregate where TAggregate : IAggregate
    {
        public TAggregate AggregateVersion { get; set; }

        public string AssemblyQualifiedTypeName { get; set; }
    }
}