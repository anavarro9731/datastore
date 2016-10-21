using System;
using System.Linq;

namespace DataStore.DataAccess.Interfaces.Events
{
    public interface IDataStoreReadEvent : IDataStoreEvent
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