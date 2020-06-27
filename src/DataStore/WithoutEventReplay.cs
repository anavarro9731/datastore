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
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    public class WithoutEventReplay : IWithoutEventReplay
    {
        private readonly ControlFunctions controlFunctions;

        private readonly IDocumentRepository dataStoreConnection;

        private readonly DataStoreOptions dataStoreOptions;

        private readonly IMessageAggregator messageAggregator;

        public WithoutEventReplay(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator messageAggregator,
            ControlFunctions controlFunctions,
            DataStoreOptions dataStoreOptions)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.messageAggregator = messageAggregator;
            this.controlFunctions = controlFunctions;
            this.dataStoreOptions = dataStoreOptions;
        }

        //* Count (no options)
        public async Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(Count), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        //* CountActive (no options)
        public async Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(CountActive), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        //* Read
        public Task<IEnumerable<T>> Read<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<WithoutReplayOptionsClientSide<T>> setOptions = null) where T : class, IAggregate, new() =>
            Read<T, DefaultWithoutReplayOptions<T>>(predicate, setOptions);

        public async Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<T>, new()
        {
            WithoutReplayOptionsLibrarySide<T> options = setOptions == null ? new O() : new O().Op(setOptions);

            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>(options);
            if (predicate != null) queryable = queryable.Where(predicate);

            var results = await this.messageAggregator
                                    .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(Read), queryable, options))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            var applySecurity = this.dataStoreOptions.Security != null && options.Identity != null;
            if (applySecurity)
            {
                results = await this.controlFunctions.AuthoriseData(results, DatabasePermissions.READ, options.Identity)
                                    .ConfigureAwait(false);
            }

            return results;
        }

        //* ReadActive
        public async Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<T>, new()
        {
            predicate = predicate.And(a => a.Active);

            WithoutReplayOptionsLibrarySide<T> options = setOptions == null ? new O() : new O().Op(setOptions);

            var queryable = this.dataStoreConnection.CreateDocumentQuery<T>(options).Where(predicate);

            var results = await this.messageAggregator
                                    .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActive), queryable, options))
                                    .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);

            var applySecurity = this.dataStoreOptions.Security != null && options.Identity != null;
            if (applySecurity)
            {
                results = await this.controlFunctions.AuthoriseData(results, DatabasePermissions.READ, options.Identity)
                                    .ConfigureAwait(false);
            }

            return results;
        }

        public Task<IEnumerable<T>> ReadActive<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<WithoutReplayOptionsClientSide<T>> setOptions = null) where T : class, IAggregate, new() =>
            ReadActive<T, DefaultWithoutReplayOptions<T>>(predicate, setOptions);

        //* ReadById
        public async Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<T>, new()
        {
            WithoutReplayOptionsLibrarySide<T> options = setOptions == null ? new O() : new O().Op(setOptions);

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadById), modelId))
                                   .To(this.dataStoreConnection.GetItemAsync<T>).ConfigureAwait(false);

            var applySecurity = this.dataStoreOptions.Security != null && options.Identity != null;
            if (applySecurity)
            {
                result = await this.controlFunctions.AuthoriseDatum(result, DatabasePermissions.READ, options.Identity)
                                   .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> ReadById<T>(Guid modelId, Action<WithoutReplayOptionsClientSide<T>> setOptions = null)
            where T : class, IAggregate, new() =>
            ReadById<T, DefaultWithoutReplayOptions<T>>(modelId, setOptions);
    }
}