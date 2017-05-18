namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreQueryCapabilities
    {
        Task<bool> Exists(Guid id);

        Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : class, IAggregate, new();

        Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new();
    }
}