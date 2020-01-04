namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreQueryCapabilities
    {
        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T>(string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T>(string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadActiveById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new();
    }
}