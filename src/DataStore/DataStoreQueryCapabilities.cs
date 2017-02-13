using PalmTree.Infrastructure.EventAggregator;
using PalmTree.Infrastructure.Interfaces;

namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Models.Messages.Events;

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly IEventAggregator eventAggregator;
        private readonly EventReplay eventReplay;

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventReplay = new EventReplay(eventAggregator);
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;

            if (eventAggregator.Events.OfType<IDataStoreWriteEvent>()
                .ToList()
                .Exists(e => e.AggregateId == id && e.GetType() == typeof(AggregateHardDeleted<>)))
                return false;

            return await eventAggregator.Store(new AggregateQueriedById(nameof(Exists), id)).ForwardToAsync(DbConnection.Exists);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate
        {
            var results = await ReadInternal(queryableExtension);

            return eventReplay.ApplyAggregateEvents(results, false);
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> activeOnlyQueryableExtension = q =>
            {
                if (queryableExtension != null)
                    q = queryableExtension(q);

                q = q.Where(a => a.Active);

                return q;
            };

            var results = await ReadInternal(activeOnlyQueryableExtension);

            return eventReplay.ApplyAggregateEvents(results, true);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = q => q.Where(a => a.id == modelId && a.Active);
            var results = await ReadInternal(queryableExtension);

            return eventReplay.ApplyAggregateEvents(results, true).Single();
        }

        private async Task<IEnumerable<T>> ReadInternal<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension) where T : IAggregate
        {
            var queryable = DbConnection.CreateDocumentQuery<T>();
            if (queryableExtension != null)
                queryable = queryableExtension(queryable);

            var results = await eventAggregator.Store(new AggregatesQueried<T>(nameof(ReadInternal), queryable)).ForwardToAsync(DbConnection.ExecuteQuery);

            return results;
        }
    }
}