namespace DataStore.Interfaces.Operations
{
    #region

    using System;
    using System.Linq.Expressions;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public interface IDataStoreCountOperation : IDataStoreOperation, IRequestState
    {
    }

    public interface IDataStoreCountFromQueryable<T> : IDataStoreCountOperation
    {
        Expression<Func<T, bool>> Predicate { get; set; }
        
        IOptionsLibrarySide QueryOptions { get; set; }

    }
}