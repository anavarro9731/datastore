﻿namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreQueryCapabilities
    {
        Task<bool> Exists(Guid id);

        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T, O>(Action<O> setOptions, Expression<Func<T, bool>> predicate = null)
            where T : class, IAggregate, new() where O : class, IQueryOptions, new();

        Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T, O>(Action<O> setOptions, Expression<Func<T, bool>> predicate = null)
            where T : class, IAggregate, new() where O : class, IQueryOptions, new();

        Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new();
    }
}