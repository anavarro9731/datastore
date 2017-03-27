using System;
using System.Linq;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Interfaces.Events
{
    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadEvent
    {
        IQueryable<T> Query { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadEvent
    {
        Guid Id { get; set; }
    }

    public interface IDataStoreReadEvent : IDataStoreEvent, IStateQuery
    {
    }
}