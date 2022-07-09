namespace DataStore.Interfaces.Operations
{
    using System;
    using System.Linq.Expressions;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.Options;

    public interface IDataStoreCountOperation : IDataStoreOperation, IRequestState
    {
    }

    public interface IDataStoreCountFromQueryable<T> : IDataStoreCountOperation
    {
        Expression<Func<T, bool>> Predicate { get; set; }
        
        IQueryOptions QueryOptions { get; set; }

    }
}