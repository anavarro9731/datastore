namespace DataStore.Interfaces.Events
{
    using System;
    using System.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public interface IDataStoreReadEvent : IDataStoreEvent, IGatedMessage
    {

    }

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadEvent
    {
        IQueryable<T> Query { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadEvent
    {
        Guid Id { get; set; }
    }
}