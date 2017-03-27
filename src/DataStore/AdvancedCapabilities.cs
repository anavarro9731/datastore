namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Messages.Events;
    using Models.PureFunctions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    public class AdvancedCapabilities : IAdvancedCapabilities
    {
        private readonly IDocumentRepository _dataStoreConnection;
        private readonly IMessageAggregator _messageAggregator;

        public AdvancedCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this._dataStoreConnection = dataStoreConnection;
            this._messageAggregator = messageAggregator;
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
            var transformedQueryable = queryableExtension(_dataStoreConnection.CreateDocumentQuery<T>());
            var results = await _messageAggregator.CollectAndForward(new TransformationQueried<T2>(nameof(ReadCommittedInternal), transformedQueryable)).To(_dataStoreConnection.ExecuteQuery);
            return results;
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<dynamic> ReadCommittedById(Guid modelId)
        {
            var result = await _messageAggregator.CollectAndForward(new AggregateQueriedById(nameof(ReadCommittedById), modelId)).To(_dataStoreConnection.GetItemAsync);
            return result;
        }
    }
}