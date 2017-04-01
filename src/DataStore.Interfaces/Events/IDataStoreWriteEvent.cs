using System;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Interfaces.Events
{
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

    public interface IDataStoreWriteEvent<T> : IDataStoreWriteEvent
        where T : IAggregate
    {
        T Model { get; set; }
    }

    public interface IDataStoreWriteEvent : IQueuedStateChange, IDataStoreEvent
    {
        Guid AggregateId { get; set; }
    }
}