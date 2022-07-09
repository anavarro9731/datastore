namespace DataStore.Interfaces.Operations
{
    using System;
    using System.Linq;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation where T : class, IAggregate, new()
    {
        IQueryable<T> Query { get; set; }

        IQueryOptions QueryOptions { get; set; }
    }

    public interface IDataStoreReadByIdOperation : IDataStoreReadOperation
    {
        IQueryOptions QueryOptions { get; set; }
        
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}