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

    public class DirectToDb : IDirectToDb
    {
        private readonly IDocumentRepository dataStoreConnection;

        private readonly IMessageAggregator messageAggregator;

        public DirectToDb(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.messageAggregator = messageAggregator;
        }

        public async Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(Count), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        public async Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(CountActive), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Func<IQueryOptions> queryOptions = null)
            where T : class, IAggregate, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();

            queryable = queryable.Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable, queryOptions?.Invoke()))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            return results;
        }

        public Task<T> ReadById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            return this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadById), modelId))
                       .To(this.dataStoreConnection.GetItemAsync<T>);
        }
    }
}