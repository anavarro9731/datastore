﻿namespace DataStore.Interfaces.Operations
{
    using System;
    using System.Linq;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation where T : class, IAggregate, new()
    {
        IQueryable<T> Query { get; set; }

        object QueryOptions { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadOperation
    {
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}