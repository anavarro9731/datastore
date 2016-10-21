namespace DataStore.DataAccess.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;

    public interface IDataStoreQueryCapabilities
    {
        Task<bool> Exists(Guid id);

        Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate;

        Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate;

        Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate;

        Task<Document> ReadById(Guid modelId);
    }
}