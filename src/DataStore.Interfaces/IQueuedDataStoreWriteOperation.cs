using System;
using DataStore.Interfaces.LowLevel;
using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

namespace DataStore.Interfaces
{
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