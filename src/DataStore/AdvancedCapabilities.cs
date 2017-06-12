using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.LowLevel;
using DataStore.Models.Messages;
using DataStore.Models.PureFunctions;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore
{
    public class AdvancedCapabilities : IAdvancedCapabilities
    {
        private readonly IDocumentRepository dataStoreConnection;
        private readonly IMessageAggregator messageAggregator;

        public AdvancedCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.messageAggregator = messageAggregator;
        }

        #region

        public Task<IEnumerable<T2>> ReadCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension)
            where T : class, IAggregate, new()
        {
            Guard.Against(() => queryableExtension == null,
                "Queryable cannot be null when asking for a different return type to the type being queried");

            return ReadCommittedInternal(queryableExtension);
        }

        // get a filtered list of the models from a set of active DataObjects
        public Task<IEnumerable<T2>> ReadActiveCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension)
            where T : class, IAggregate, new()
        {
            Guard.Against(() => queryableExtension == null,
                "Queryable cannot be null when asking for a different return type to the type being queried");

            Func<IQueryable<T>, IQueryable<T2>> activeOnlyQueryableExtension = q =>
            {
                q = q.Where(a => a.Active);

                return queryableExtension(q);
            };

            return ReadCommittedInternal(activeOnlyQueryableExtension);
        }

        // get a filtered list of the models from  a set of DataObjects
        public Task<T> ReadCommittedById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            return messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadCommittedById), modelId))
                .To(dataStoreConnection.GetItemAsync<T>);
        }

        #endregion

        private Task<IEnumerable<T2>> ReadCommittedInternal<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension)
            where T : class, IAggregate, new()
        {
            var transformedQueryable = queryableExtension(dataStoreConnection.CreateDocumentQuery<T>());
            return messageAggregator.CollectAndForward(
                    new TransformationQueriedOperation<T2>(nameof(ReadCommittedInternal), transformedQueryable))
                .To(dataStoreConnection.ExecuteQuery);
        }
    }
}