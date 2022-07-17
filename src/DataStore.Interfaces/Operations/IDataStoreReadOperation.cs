namespace DataStore.Interfaces.Operations
{
    #region

    using System;
    using System.Linq;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation where T : class, IAggregate, new()
    {
        IQueryable<T> Query { get; set; }

        IOptionsLibrarySide QueryOptions { get; set; }
    }

    public interface IDataStoreReadByIdOperation : IDataStoreReadOperation
    {
        IOptionsLibrarySide QueryOptions { get; set; }
        
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}