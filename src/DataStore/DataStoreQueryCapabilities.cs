namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using Models.Messages;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    //methods return the latest version of an object including uncommitted session changes

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly EventReplay eventReplay;
        private readonly IMessageAggregator messageAggregator;

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            eventReplay = new EventReplay(messageAggregator);
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        #region

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;

            if (HasBeenHardDeletedInThisSession(id)) return false;

            return await messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(Exists), id))
                .To(DbConnection.Exists);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : class, IAggregate, new()
        {
            var queryable = DbConnection.CreateDocumentQuery<T>();

            if (queryableExtension != null)
                queryable = queryableExtension(queryable);

            var results = await messageAggregator
                .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActiveById), queryable))
                .To(DbConnection.ExecuteQuery);

            return eventReplay.ApplyAggregateEvents(results, false);
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : class, IAggregate, new()
        {
            IQueryable<T> ActiveOnlyQueryableExtension(IQueryable<T> q)
            {
                return q.Where(a => a.Active);
            }

            var queryable = DbConnection.CreateDocumentQuery<T>();

            if (queryableExtension != null)
                queryable = ActiveOnlyQueryableExtension(queryableExtension(queryable));

            var results = await messageAggregator
                .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActiveById), queryable))
                .To(DbConnection.ExecuteQuery);

            return eventReplay.ApplyAggregateEvents(results, true);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await messageAggregator
                .CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadActiveById), modelId))
                .To(DbConnection.GetItemAsync<T>);

            if (result == null || !result.Active) return null;

            return eventReplay.ApplyAggregateEvents(new List<T> {result}, true).SingleOrDefault();
        }

        #endregion

        private bool HasBeenHardDeletedInThisSession(Guid id)
        {
            //if its been deleted in this session (this takes the place of eventReplay for this function)
            if (messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>()
                .ToList()
                .Exists(e => e.AggregateId == id && e.GetType() == typeof(QueuedHardDeleteOperation<>)))
                return true;
            return false;
        }
    }
}