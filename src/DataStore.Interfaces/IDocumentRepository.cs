namespace DataStore.DataAccess.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Messages.Events;
    using Microsoft.Azure.Documents;

    public interface IDocumentRepository : IDisposable
    {
        Task<T> AddAsync<T>(AggregateAdded<T> aggregateAdded) where T : IHaveAUniqueId;

        IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema;
        
        Task<IEnumerable<T>> ExecuteQuery<T>(AggregatesQueried<T> aggregatesQueried) where T : IHaveAUniqueId;

        Task<bool> Exists(AggregateQueriedById aggregateQueriedById);

        Task<T> GetItemAsync<T>(AggregateQueriedById aggregateQueriedById) where T : IHaveAUniqueId;

        Task<Document> GetItemAsync(AggregateQueriedById aggregateQueriedById);

        Task<T> UpdateAsync<T>(AggregateUpdated<T> aggregateUpdated) where T : IHaveAUniqueId;
    
        Task<T> DeleteHardAsync<T>(AggregateHardDeleted<T> aggregateHardDeleted) where T : IHaveAUniqueId;

        Task<T> DeleteSoftAsync<T>(AggregateSoftDeleted<T> aggregateSoftDeleted) where T : IHaveAUniqueId;
    }
}