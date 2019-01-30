namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDocumentRepository : IDisposable
    {
        Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new();

        IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new();

        Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried);

        Task<bool> Exists(IDataStoreReadById aggregateQueriedById);

        Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new();

        Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new();

        Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new();
    }
}