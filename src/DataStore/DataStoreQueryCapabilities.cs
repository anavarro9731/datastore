namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using Messages.Events;
    using Microsoft.Azure.Documents;

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            this.DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;
            return await _eventAggregator.Store(new AggregateQueriedById(nameof(Exists), id)).ForwardToAsync(DbConnection.Exists);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate
        {
            var queryable = this.DbConnection.CreateDocumentQuery<T>();
            if (queryableExtension != null)
            {
                queryable = queryableExtension(queryable);
            }

            var results = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(Read), queryable)).ForwardToAsync(DbConnection.ExecuteQuery);
            return results;
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension2 = (q) =>
                {
                    if (queryableExtension != null)
                    {
                        q = queryableExtension(q);
                    }

                    q = q.Where(a => a.Active);

                    return q;
                };
            return await this.Read<T>(queryableExtension2);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = (q) => q.Where(a => a.id == modelId && a.Active);
            var results = await this.Read<T>(queryableExtension);
            return results.Single();
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<Document> ReadById(Guid modelId)
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(ReadById), modelId)).ForwardToAsync(DbConnection.GetItemAsync);
            return result;
        }
    }
}