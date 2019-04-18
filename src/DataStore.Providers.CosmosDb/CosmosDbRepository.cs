namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public class CosmosDbRepository : IDocumentRepository, IResetData
    {
        private readonly DocumentClient client;

        private readonly Uri collectionUri;

        private readonly CosmosSettings settings;

        public CosmosDbRepository(CosmosSettings settings)
        {
            this.collectionUri = UriFactory.CreateDocumentCollectionUri(settings.DatabaseName, settings.DatabaseName);
            this.client = new DocumentClient(new Uri(settings.EndpointUrl), settings.AuthKey);
            this.settings = settings;
        }

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded == null || aggregateAdded.Model == null)
            {
                throw new ArgumentNullException(nameof(aggregateAdded));
            }

            var result = await this.client.CreateDocumentAsync(this.collectionUri, aggregateAdded.Model).ConfigureAwait(false);

            aggregateAdded.StateOperationCost = result.RequestCharge;
        }

        public async Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateDocumentQuery<T>();

            var count = await query.Where(aggregatesCounted.Predicate).CountAsync().ConfigureAwait(false);

            return count;
        }

        public IQueryable<T> CreateDocumentQuery<T>(IQueryOptions<T> queryOptions = null) where T : class, IEntity, new()
        {
            var schema = typeof(T).FullName;
            var query = this.client.CreateDocumentQuery<T>(
                this.collectionUri,
                new FeedOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session, //should be the default anyway
                    EnableCrossPartitionQuery = true,
                    PartitionKey = new PartitionKey(Aggregate.PartitionKeyValue)
                }).Where(i => i.Schema == schema);

            return query;
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var docLink = CreateDocumentSelfLinkFromId(aggregateHardDeleted.Model.id);

            var result = await this.client.DeleteDocumentAsync(
                             docLink,
                             new RequestOptions
                             {
                                 PartitionKey = new PartitionKey(Aggregate.PartitionKeyValue)
                             }).ConfigureAwait(false);

            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var results = new List<T>();

            var documentQuery = aggregatesQueried.Query.AsDocumentQuery();

            while (documentQuery.HasMoreResults)
            {
                var result = await documentQuery.ExecuteNextAsync<T>().ConfigureAwait(false);

                aggregatesQueried.StateOperationCost += result.RequestCharge;

                results.AddRange(result);
            }

            return results;
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var query = CreateDocumentQuery<T>();
            var count = await query.Where(d => d.id == aggregateQueriedById.Id).CountAsync().ConfigureAwait(false);
            if (count == 0) return default;

            var result = await this.client.ReadDocumentAsync<T>(
                             CreateDocumentSelfLinkFromId(aggregateQueriedById.Id),
                             new RequestOptions
                             {
                                 PartitionKey = new PartitionKey(Aggregate.PartitionKeyValue)
                             }).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var result = await this.client.ReplaceDocumentAsync(
                             CreateDocumentSelfLinkFromId(aggregateUpdated.Model.id),
                             aggregateUpdated.Model,
                             new RequestOptions
                             {
                                 PartitionKey = new PartitionKey(Aggregate.PartitionKeyValue)
                             }).ConfigureAwait(false);

            aggregateUpdated.StateOperationCost = result.RequestCharge;
        }

        private Uri CreateDocumentSelfLinkFromId(Guid id)
        {
            if (Guid.Empty == id)
            {
                throw new ArgumentException("id is required for update/delete/read operation");
            }

            var docLink = UriFactory.CreateDocumentUri(this.settings.DatabaseName, this.settings.DatabaseName, id.ToString());
            return docLink;
        }

        async Task IResetData.NonTransactionalReset()
        {
            await CosmosDbUtilities.ResetDatabase(this.settings);
        }
    }
}