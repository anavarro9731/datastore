
namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Models.Messages.Events;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    //methods return the latest version of an object including uncommitted session changes

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly IMessageAggregator messageAggregator;
        private readonly EventReplay eventReplay;

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            eventReplay = new EventReplay(messageAggregator);
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;

            //if its been deleted in this session (this takes the place of eventReplay for this function)
            if (messageAggregator.AllMessages.OfType<IDataStoreWriteEvent>()
                .ToList()
                .Exists(e => e.AggregateId == id && e.GetType() == typeof(AggregateHardDeleted<>)))
                return false;

            return await messageAggregator.CollectAndForward(new AggregateQueriedById(nameof(Exists), id)).To(DbConnection.Exists);
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

            var results = await messageAggregator.CollectAndForward(new AggregatesQueried<T>(nameof(ReadInternal), queryable)).To(DbConnection.ExecuteQuery);

            return results;
        }
    }
}