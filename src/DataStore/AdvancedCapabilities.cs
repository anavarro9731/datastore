using PalmTree.Infrastructure.Interfaces;
using PalmTree.Infrastructure.PureFunctions;

namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Azure.Documents;
    using Models.Messages.Events;

    public class AdvancedCapabilities : IAdvancedCapabilities
    {
        private readonly IDocumentRepository dataStoreConnection;
        private readonly IEventAggregator eventAggregator;

        public AdvancedCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.eventAggregator = eventAggregator;
        }

        public async Task<IEnumerable<T2>> ReadCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : IAggregate
        {
            Guard.Against(() => queryableExtension == null, "Queryable cannot be null when asking for a different return type to the type being queried");

            var results = await ReadCommittedInternal(queryableExtension);

            return results;
        }

        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T2>> ReadActiveCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : IAggregate
        {
            Guard.Against(() => queryableExtension == null, "Queryable cannot be null when asking for a different return type to the type being queried");

            Func<IQueryable<T>, IQueryable<T2>> activeOnlyQueryableExtension = (q) =>
            {
                q = q.Where(a => a.Active);

                return queryableExtension(q);
            };

            var results = await this.ReadCommittedInternal(activeOnlyQueryableExtension);

            return results;
        }

        private async Task<IEnumerable<T2>> ReadCommittedInternal<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : IAggregate
        {
            var transformedQueryable = queryableExtension(dataStoreConnection.CreateDocumentQuery<T>());
            var results = await eventAggregator.Store(new TransformationQueried<T2>(nameof(ReadCommittedInternal), transformedQueryable)).ForwardToAsync<IEnumerable<T2>>(dataStoreConnection.ExecuteQuery);
            return results;
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<Document> ReadCommittedById(Guid modelId)
        {
            var result = await eventAggregator.Store(new AggregateQueriedById(nameof(ReadCommittedById), modelId)).ForwardToAsync(dataStoreConnection.GetItemAsync);
            return result;
        }
    }
}