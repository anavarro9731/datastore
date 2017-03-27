using System;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Interfaces.Events
{
    public interface IDataStoreWriteEvent<T> : IDataStoreWriteEvent
        where T : IAggregate
    {
        T Model { get; }
    }

    public interface IDataStoreWriteEvent : IQueuedStateChange, IDataStoreEvent
    {
        Guid AggregateId { get; }
    }
}