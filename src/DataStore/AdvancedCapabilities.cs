namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;

    public class AdvancedCapabilities : IAdvancedCapabilities
    {
        private readonly IDocumentRepository dataStoreConnection;

        private readonly IMessageAggregator messageAggregator;

        public AdvancedCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.messageAggregator = messageAggregator;
        }

        // get a filtered list of the models from a set of active DataObjects
        public Task<IEnumerable<T2>> ReadActiveCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : class, IAggregate, new()
        {
            Guard.Against(() => queryableExtension == null, "Queryable cannot be null when asking for a different return type to the type being queried");

            Func<IQueryable<T>, IQueryable<T2>> activeOnlyQueryableExtension = q =>
                {
                q = q.Where(a => a.Active);

                return queryableExtension(q);
                };

            return ReadCommittedInternal(activeOnlyQueryableExtension);
        }

        public Task<IEnumerable<T2>> ReadCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : class, IAggregate, new()
        {
            Guard.Against(() => queryableExtension == null, "Queryable cannot be null when asking for a different return type to the type being queried");

            return ReadCommittedInternal(queryableExtension);
        }

        // get a filtered list of the models from  a set of DataObjects
        public Task<T> ReadCommittedById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            return this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadCommittedById), modelId))
                       .To(this.dataStoreConnection.GetItemAsync<T>);
        }

        private Task<IEnumerable<T2>> ReadCommittedInternal<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : class, IAggregate, new()
        {
            var transformedQueryable = queryableExtension(this.dataStoreConnection.CreateDocumentQuery<T>());
            return this.messageAggregator.CollectAndForward(new TransformationQueriedOperation<T2>(nameof(ReadCommittedInternal), transformedQueryable))
                       .To(this.dataStoreConnection.ExecuteQuery);
        }
    }
}