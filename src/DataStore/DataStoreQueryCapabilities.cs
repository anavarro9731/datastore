﻿namespace DataStore
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

    //methods return the latest version of an object including uncommitted session changes

    public class DataStoreQueryCapabilities
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

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate, O options, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsLibrarySide, new()
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

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, queryable))
                                    .To(DbConnection.ExecuteQuery).ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate, O options, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsLibrarySide, new()

        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var queryable = DbConnection.CreateDocumentQuery<T>().Where(predicate);

            var results = await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, queryable))
                                    .To(DbConnection.ExecuteQuery).ConfigureAwait(false);

            return this.eventReplay.ApplyAggregateEvents(results, predicate.Compile());
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T, O>(Guid modelId, O options, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsLibrarySide, new()

        {
            if (modelId == Guid.Empty) return null;

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, modelId))
                                   .To(DbConnection.GetItemAsync<T>).ConfigureAwait(false);

            bool Predicate(T a) => a.Active && a.id == modelId;

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

        public async Task<T> ReadById<T, O>(Guid modelId, O options, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsLibrarySide, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, modelId))
                                   .To(DbConnection.GetItemAsync<T>).ConfigureAwait(false);

            bool Predicate(T a) => a.id == modelId;

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
    }
}