namespace DataStore.Interfaces
{
    using System;
    using System.Linq;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation where T : class, IAggregate, new()
    {
        IQueryable<T> Query { get; set; }
        IQueryOptions<T> QueryOptions { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadOperation
    {
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}