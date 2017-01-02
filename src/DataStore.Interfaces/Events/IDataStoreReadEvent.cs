namespace DataStore.Interfaces.Events
{
    using System;
    using System.Linq;

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