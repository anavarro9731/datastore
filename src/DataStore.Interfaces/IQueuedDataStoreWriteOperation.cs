namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    public interface IQueuedDataStoreWriteOperation<T> : IQueuedDataStoreWriteOperation where T : class, IAggregate, new()
    {

        new T PreviousModel { get; set; }

        new T NewModel { get; set; }
    }

    public interface IQueuedDataStoreWriteOperation : IQueuedStateChange
    {

        IAggregate PreviousModel { get; }

        IAggregate NewModel { get; }

        Guid AggregateId { get; set; }

        DateTime Created { get; set; }
    }
}