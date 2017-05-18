﻿using DataStore.Models.Messages;
using DataStore.Models.PureFunctions.Extensions;

namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    // Not sure if eventreplay makes sense in this class, needs review currently its not implemented.
    // It's also questionable what happens to events subsquent to a hard-delete in a session, how does it error?

    // All methods return the version of the object before it was deleted, for soft delete this is probably
    // a bit confusing, but trying to mark them in this class raises issues with duplication of logic in 
    // the documentRepository and matching the timestamps. Needs review.

    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IMessageAggregator messageAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        #region IDataStoreDeleteCapabilities Members

        public async Task<T> DeleteHardById<T>(Guid id) where T : class, IAggregate, new()
        {
            var result = await messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(DeleteHardById), id, typeof(T)))
                .To(DsConnection.GetItemAsync<T>);

            messageAggregator.Collect(new QueuedHardDeleteOperation<T>(nameof(DeleteHardById), result, DsConnection, messageAggregator));

            return result.Clone();
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            var objects = await messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(DeleteHardWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .To(DsConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                messageAggregator.Collect(new QueuedHardDeleteOperation<T>(nameof(DeleteHardWhere), dataObject, DsConnection, messageAggregator));

            return dataObjects.Select(d => d.Clone());
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : class, IAggregate, new()
        {
            var result = await messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(DeleteSoftById), id, typeof(T)))
                .To(DsConnection.GetItemAsync<T>);

            messageAggregator.Collect(new QueuedSoftDeleteOperation<T>(nameof(DeleteSoftById), result, DsConnection, messageAggregator));

            return result.Clone();
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            var objects = await messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(DeleteSoftWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .To(DsConnection.ExecuteQuery);
            
            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                messageAggregator.Collect(new QueuedSoftDeleteOperation<T>(nameof(DeleteSoftWhere), dataObject, DsConnection, messageAggregator));

            return dataObjects.Select(o => o.Clone());
        }

        #endregion
    }
}