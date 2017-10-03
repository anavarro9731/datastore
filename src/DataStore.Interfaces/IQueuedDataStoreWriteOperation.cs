namespace DataStore.Interfaces
{
    using System;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    public interface IQueuedDataStoreWriteOperation<T> : IQueuedDataStoreWriteOperation where T : class, IAggregate, new()
    {
        T Model { get; set; }
    }

    public interface IQueuedDataStoreWriteOperation : IQueuedStateChange
    {
        Guid AggregateId { get; set; }

        DateTime Created { get; set; }
    }
}