namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Events;
    using Microsoft.Azure.Documents;
    using ServiceApi.Interfaces.LowLevel;

    public interface IDocumentRepository : IDisposable
    {
        Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate;

        IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema;

        Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried);

        Task<bool> Exists(IDataStoreReadById aggregateQueriedById);

        Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId;

        Task<Document> GetItemAsync(IDataStoreReadById aggregateQueriedById);

        Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate;
    
        Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate;

        Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate;
    }
}