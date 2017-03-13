namespace DataStore.Interfaces.Events
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public interface IDataStoreWriteEvent<T> : IDataStoreWriteEvent, IDataStoreEvent where T : IAggregate
    {
        T Model { get; }
    }

    public interface IDataStoreWriteEvent : IQueuedStateChange
    {
        Guid AggregateId { get; }
    }
}