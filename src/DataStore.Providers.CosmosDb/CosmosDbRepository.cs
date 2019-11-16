namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public class CosmosDbRepository : IDocumentRepository, IResetData
    {
        private DocumentClient client;

        private readonly Uri collectionUri;

        private readonly CosmosSettings settings;

        public CosmosDbRepository(CosmosSettings settings)
        {
            this.collectionUri = UriFactory.CreateDocumentCollectionUri(settings.DatabaseName, settings.DatabaseName);
            this.settings = settings;
            ResetClient();
        }

        private void ResetClient()
        {
            this.client = new DocumentClient(new Uri(this.settings.EndpointUrl), this.settings.AuthKey);
        }

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded?.Model == null)
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
                    RequestContinuation = (queryOptions as IContinueAndTake<T>)?.CurrentContinuationToken?.ToString(),
                    MaxItemCount = (queryOptions as IContinueAndTake<T>)?.MaxTake,
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

            {
                if (aggregatesQueried.QueryOptions is IOrderBy<T> orderByOptions)
                {
                    aggregatesQueried.Query = orderByOptions.AddOrderBy(aggregatesQueried.Query);
                    if (orderByOptions.OrderByParameters.Count > 1)
                    {
                        await CreateIndexes(orderByOptions.OrderByParameters);
                    }
                }

                var documentQuery = aggregatesQueried.Query.AsDocumentQuery();

                while (HaveLessRecordsThanUserRequested() && documentQuery.HasMoreResults)
                {
                    var result = await documentQuery.ExecuteNextAsync<T>().ConfigureAwait(false);

                    aggregatesQueried.StateOperationCost += result.RequestCharge;

                    if (aggregatesQueried.StateOperationCost > this.settings.MaxQueryCostInRus)
                        throw new Exception($"Query cost exceeds limit of {this.settings.MaxQueryCostInRus} RUs, abandoning");

                    results.AddRange(result);

                    SetContinuationToken(result);  
                }

                return results;
            }

            bool HaveLessRecordsThanUserRequested()
            {
                var userRequestedLimit = (aggregatesQueried.QueryOptions as IContinueAndTake<T>)?.MaxTake;

                return userRequestedLimit == null || results.Count < userRequestedLimit;
            }

            void SetContinuationToken(FeedResponse<T> result)
            {
                if (aggregatesQueried.QueryOptions is IContinueAndTake<T> continueAndTakeOptions && continueAndTakeOptions.MaxTake != null)
                {
                    continueAndTakeOptions.NextContinuationToken = new ContinuationToken(result.ResponseContinuation);
                }
            }

            async Task CreateIndexes(List<(string, bool)> fieldName_IsDescending)
            {
                // Retrieve the container's details
                ResourceResponse<DocumentCollection> containerResponse = await client.ReadDocumentCollectionAsync(this.collectionUri);
                // Add a composite index
                var compositePaths = new Collection<CompositePath>();

                foreach (var valueTuple in fieldName_IsDescending)
                {
                    compositePaths.Add(new CompositePath()
                    {
                        Path = $"/{valueTuple.Item1}",
                        Order = valueTuple.Item2 ? CompositePathSortOrder.Descending : CompositePathSortOrder.Ascending
                    });
                }

                containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(compositePaths);
                // Update container with changes
                await client.ReplaceDocumentCollectionAsync(containerResponse.Resource);

                long indexTransformationProgress;
                do
                {
                    // retrieve the container's details
                    ResourceResponse<DocumentCollection> container = await client.ReadDocumentCollectionAsync(
                                                                         this.collectionUri,
                                                                         new RequestOptions
                                                                         {
                                                                             PopulateQuotaInfo = true
                                                                         });
                    // retrieve the index transformation progress from the result
                    indexTransformationProgress = container.IndexTransformationProgress;
                }
                while (indexTransformationProgress < 100);

            }
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var query = CreateDocumentQuery<T>();
            var count = await query.Where(d => d.id == aggregateQueriedById.Id).CountAsync().ConfigureAwait(false);
            if (count == 0) return default(T);

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
            ResetClient();
        }
    }
}