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

    public class WithoutEventReplay : IWithoutEventReplay
    {
        private readonly IDocumentRepository dataStoreConnection;

        private readonly IMessageAggregator messageAggregator;

        public WithoutEventReplay(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
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

        public async Task<IEnumerable<T>> Read<T>() where T : class, IAggregate, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            return results;
        }

        public async Task<IEnumerable<T>> Read<T, O>(Action<O> setOptions) where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();

            var options = new O();
            setOptions(options);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable, options))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            return results;
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();
            queryable = queryable.Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            return results;
        }

        public async Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();
            queryable = queryable.Where(predicate);

            var options = new O();
            setOptions(options);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable, options))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            return results;
        }

        public async Task<IEnumerable<T>> ReadActive<T>() where T : class, IAggregate, new() 
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);
            return results;
        }

        public async Task<IEnumerable<T>> ReadActive<T, O>(Action<O> setOptions) where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new()
        {
            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>();

            O options = null;
            if (setOptions != null)
            {
                options = new O();
                setOptions(options);
            }

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable, options))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);
            return results;
        }

        public async Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            predicate = predicate.And(a => a.Active);

            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>().Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);
            return results;
        }

        public async Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new()
        {
            predicate = predicate.And(a => a.Active);

            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>().Where(predicate);

            O options = null;
            if (setOptions != null)
            {
                options = new O();
                setOptions(options);
            }

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable, options))
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