namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IAggregateHistoryItem<TAggregate> where TAggregate : IAggregate
    {
        TAggregate AggregateVersion { get; set; }

        Guid? UnitOfWorkResponsibleForStateChange { get; set; }
    }

    public class AggregateHistoryItem<TAggregate> : Aggregate, IAggregateHistoryItem<TAggregate> where TAggregate : IAggregate
    {
        public TAggregate AggregateVersion { get; set; }

        public Guid? UnitOfWorkResponsibleForStateChange { get; set; }
    }
}