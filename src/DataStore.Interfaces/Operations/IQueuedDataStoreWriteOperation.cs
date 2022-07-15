namespace DataStore.Interfaces.Operations
{
    #region

    using System;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    #endregion

    public interface IQueuedDataStoreWriteOperation<T> : IQueuedDataStoreWriteOperation where T : class, IAggregate, new()
    {
        new T NewModel { get; set; }

        new T PreviousModel { get; set; }
    }

    public interface IQueuedDataStoreWriteOperation : IQueuedStateChange
    {
        Guid AggregateId { get; set; }

        DateTime Created { get; set; }

        long? LastModified { get; }

        IAggregate NewModel { get; }

        IAggregate PreviousModel { get; }
    }
}