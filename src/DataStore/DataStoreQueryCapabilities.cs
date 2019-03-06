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

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            this.eventReplay = new EventReplay(messageAggregator);
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;

            if (HasBeenHardDeletedInThisSession(id)) return false;

            return await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(Exists), id)).To(DbConnection.Exists)
                             .ConfigureAwait(false);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
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

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable)).To(DbConnection.ExecuteQuery)
                                    .ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        public Task<IEnumerable<T>> Read<T>() where T : class, IAggregate, new()
        {
            return Read<T>(null);
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var queryable = DbConnection.CreateDocumentQuery<T>().Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable))
                                    .To(DbConnection.ExecuteQuery).ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        public Task<IEnumerable<T>> ReadActive<T>() where T : class, IAggregate, new()
        {
            return ReadActive<T>(null);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadActiveById), modelId))
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

        private bool HasBeenHardDeletedInThisSession(Guid id)
        {
            //if its been deleted in this session (this takes the place of eventReplay for this function)
            if (this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().ToList()
                    .Exists(e => e.AggregateId == id && e.GetType() == typeof(QueuedHardDeleteOperation<>)))
            {
                return true;
            }

            return false;
        }
    }
}