﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.LowLevel;
using DataStore.Models.Messages;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore
{
    using System.Linq.Expressions;

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
                .To(DbConnection.Exists)
                .ConfigureAwait(false);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T,bool>> predicate = null)
            where T : class, IAggregate, new()
        {
            var queryable = DbConnection.CreateDocumentQuery<T>();

            if (predicate != null) queryable = queryable.Where(predicate);
                
            var results = await messageAggregator
                .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActiveById), queryable))
                .To(DbConnection.ExecuteQuery)
                .ConfigureAwait(false);

            return eventReplay.ApplyAggregateEvents(results, false);
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null)
            where T : class, IAggregate, new()
        {
            var queryable = DbConnection.CreateDocumentQuery<T>().Where(a => a.Active);

            if (predicate != null) queryable = queryable.Where(predicate);

            var results = await messageAggregator
                .CollectAndForward(new AggregatesQueriedOperation<T>(nameof(ReadActiveById), queryable))
                .To(DbConnection.ExecuteQuery)
                .ConfigureAwait(false);

            return eventReplay.ApplyAggregateEvents(results, true);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            if (modelId == Guid.Empty) return null;

            var result = await messageAggregator
                .CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadActiveById), modelId))
                .To(DbConnection.GetItemAsync<T>)
                .ConfigureAwait(false);

            if (result == null || !result.Active)
            {
                var replayResult = eventReplay.ApplyAggregateEvents(new List<T>(), true).SingleOrDefault();
                return replayResult;
            }

            return eventReplay.ApplyAggregateEvents(new List<T>
                {
                    result
                }, true)
                .SingleOrDefault();
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