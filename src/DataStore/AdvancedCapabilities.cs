namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using DataAccess.Models.Messages.Events;
    using Infrastructure.PureFunctions.PureFunctions;

    public class AdvancedCapabilities : IAdvancedCapabilities
    {
        private readonly IDocumentRepository _dataStoreConnection;
        private readonly IEventAggregator _eventAggregator;

        public AdvancedCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _dataStoreConnection = dataStoreConnection;
            _eventAggregator = eventAggregator;
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
            var results = await _eventAggregator.Store(new TransformationQueried<T2>(nameof(ReadCommittedInternal), transformedQueryable)).ForwardToAsync<IEnumerable<T2>>(_dataStoreConnection.ExecuteQuery);
            return results;
        }
    }
}