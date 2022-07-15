namespace DataStore.Interfaces.Operations
{
    #region

    using System;
    using System.Linq.Expressions;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.Options;

    #endregion

    public interface IDataStoreCountOperation : IDataStoreOperation, IRequestState
    {
    }

    public interface IDataStoreCountFromQueryable<T> : IDataStoreCountOperation
    {
        Expression<Func<T, bool>> Predicate { get; set; }
        
        IQueryOptions QueryOptions { get; set; }

    }
}