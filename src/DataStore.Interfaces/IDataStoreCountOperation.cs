namespace DataStore.Interfaces
{
    using System;
    using System.Linq.Expressions;
    using CircuitBoard.Messages;

    public interface IDataStoreCountOperation : IDataStoreOperation, IRequestState
    {
    }

    public interface IDataStoreCountFromQueryable<T> : IDataStoreCountOperation
    {
        Expression<Func<T, bool>> Predicate { get; set; }
    }
}