namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Events;
    using LowLevel;

    public interface IDocumentRepository : IDisposable
    {
        Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new();

        IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new();

        Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried);

        Task<bool> Exists(IDataStoreReadById aggregateQueriedById);

        Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new();

        Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new();

        Task DeleteHardAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new();

        Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new();
    }
}