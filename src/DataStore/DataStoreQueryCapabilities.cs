namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;

    //methods return the latest version of an object including uncommitted session changes

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly EventReplay eventReplay;

        private readonly IMessageAggregator messageAggregator;

        public DataStoreQueryCapabilities(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            this.eventReplay = new EventReplay(messageAggregator);
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var queryable = DbConnection.CreateDocumentQuery<T>();

            if (predicate != null)
            {
                // Only pass non null and non constant predicts to the Where() method
                queryable = queryable.Where(predicate);
            }
            else
            {
                // Set the predicate to a constant expression to satisfy EventReplay but do not use constant expressions against actual DB queries as some providers don't accept this
                predicate = a => true;
            }

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, queryable)).To(DbConnection.ExecuteQuery).ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        public Task<IEnumerable<T>> Read<T>(string methodName = null) where T : class, IAggregate, new()
        {
            return Read<T>(null, methodName);
        }

        public async Task<T> ReadById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, modelId))
                                   .To(DbConnection.GetItemAsync<T>).ConfigureAwait(false);

            bool Predicate(T a)
            {
                return a.id == modelId;
            }

            if (result == null)
            {
                var replayResult = this.eventReplay.ApplyAggregateEvents(new List<T>(), Predicate).SingleOrDefault();
                return replayResult;
            }

            return this.eventReplay.ApplyAggregateEvents(
                new List<T>
                {
                    result
                },
                Predicate).SingleOrDefault();
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var queryable = DbConnection.CreateDocumentQuery<T>().Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, queryable))
                                    .To(DbConnection.ExecuteQuery).ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        public Task<IEnumerable<T>> ReadActive<T>(string methodName = null) where T : class, IAggregate, new()
        {
            return ReadActive<T>(null, methodName);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, modelId))
                                   .To(DbConnection.GetItemAsync<T>).ConfigureAwait(false);

            bool Predicate(T a)
            {
                return a.Active && a.id == modelId;
            }

            if (result == null || !result.Active)
            {
                var replayResult = this.eventReplay.ApplyAggregateEvents(new List<T>(), Predicate).SingleOrDefault();
                return replayResult;
            }

            return this.eventReplay.ApplyAggregateEvents(
                new List<T>
                {
                    result
                },
                Predicate).SingleOrDefault();
        }

    }
}