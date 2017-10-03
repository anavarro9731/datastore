namespace DataStore.Interfaces
{
    using System;
    using System.Linq;
    using CircuitBoard.Messages;

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation
    {
        IQueryable<T> Query { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadOperation
    {
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}