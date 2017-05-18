namespace DataStore.Interfaces.Events
{
    using System;
    using LowLevel;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

    public interface IQueuedDataStoreWriteOperation<T> : IQueuedDataStoreWriteOperation
        where T : class, IAggregate, new()
    {
        T Model { get; set; }
    }

    public interface IQueuedDataStoreWriteOperation : IQueuedStateChange
    {
        DateTime Created { get; set; }
        Guid AggregateId { get; set; }
    }
}