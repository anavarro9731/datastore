namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Models.PureFunctions.Extensions;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Newtonsoft.Json.Linq;

    public class CosmosDbRepository : IDocumentRepository, IResetData
    {
        private readonly Container container;

        private readonly CosmosSettings settings;

        private CosmosClient client;

        public CosmosDbRepository(CosmosSettings settings)
        {
            CosmosDbUtilities.CreateClient(settings, out this.client);
            this.container = this.client.GetContainer(settings.DatabaseName, settings.ContainerName);
            this.settings = settings;
            ResetClient();
        }

        public IDatabaseSettings ConnectionSettings => this.settings;

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded?.Model == null)
            {
                throw new ArgumentNullException(nameof(aggregateAdded));
            }

            var result = await this.container.CreateItemAsync(aggregateAdded.Model).ConfigureAwait(false);

            aggregateAdded.Model.Etag = result.ETag; //- update it

            aggregateAdded.StateOperationCost = result.RequestCharge;
        }

        public async Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateQueryable<T>();

            var count = await query.Where(aggregatesCounted.Predicate).CountAsync().ConfigureAwait(false);

            return count;
        }

        public IQueryable<T> CreateQueryable<T>(object queryOptions = null) where T : class, IAggregate, new()
        {
            var schema = typeof(T).FullName;
            var query = this.container.GetItemLinqQueryable<T>().Where(i => i.Schema == schema);

            return query;
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var result = await this.container.DeleteItemAsync<T>(
                             aggregateHardDeleted.Model.id.ToString(),
                             new PartitionKey(Aggregate.PartitionKeyValue)).ConfigureAwait(false);

            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
            aggregateHardDeleted.Model.Etag = "item was deleted";
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
            where T : class, IAggregate, new()
        {
            var results = new List<T>();
            {
                if (aggregatesQueried.QueryOptions is WithoutReplayOptionsLibrarySide<T> orderByOptions)
                {
                    aggregatesQueried.Query = orderByOptions.AddOrderBy(aggregatesQueried.Query);
                    if (orderByOptions.OrderByParameters.Count > 1)
                    {
                        await CreateIndexes(orderByOptions.OrderByParameters).ConfigureAwait(false);
                    }
                }

                /* it would be quite a lot easier to just use the aggregatsQueried.Query.ToIterator()
                but that would give you a typed iterator, and no way to get back an untyped feed response
                which you must have in order to at the udpated ETag and copy it back to our customer eTag property.
                So for now we convert the CosmosLINQQueryable into a generic one. */

                using (var setIterator = this.container.GetItemQueryIterator<dynamic>(
                    aggregatesQueried.Query.ToQueryDefinition(),
                    (aggregatesQueried.QueryOptions as WithoutReplayOptionsLibrarySide<T>)?.CurrentContinuationToken?.ToString(),
                    new QueryRequestOptions
                    {
                        MaxItemCount = (aggregatesQueried.QueryOptions as WithoutReplayOptionsLibrarySide<T>)?.MaxTake,
                        ConsistencyLevel = ConsistencyLevel.Session, //should be the default anyway
                        PartitionKey = new PartitionKey(Aggregate.PartitionKeyValue)
                    }))
                {
                    while (HaveLessRecordsThanUserRequested() && setIterator.HasMoreResults)
                    {
                        var feedResponseEnumerable = await setIterator.ReadNextAsync();

                        aggregatesQueried.StateOperationCost += feedResponseEnumerable.RequestCharge;

                        if (aggregatesQueried.StateOperationCost > this.settings.MaxQueryCostInRus)
                        {
                            throw new Exception($"Query cost exceeds limit of {this.settings.MaxQueryCostInRus} RUs, abandoning");
                        }

                        //set updated etag
                        var typedResponses = feedResponseEnumerable.Select(
                            feedItem =>
                                {
                                var asT = ((JObject)feedItem).ToObject<T>()
                                                             /* set Etag */
                                                             .Op(t => t.As<IHaveAnETag>().Etag = feedItem.ETag);
                                return asT;
                                });

                        results.AddRange(typedResponses);

                        SetContinuationToken(feedResponseEnumerable);
                    }
                }

                return results;
            }

            bool HaveLessRecordsThanUserRequested()
            {
                var userRequestedLimit = (aggregatesQueried.QueryOptions as WithoutReplayOptionsLibrarySide<T>)?.MaxTake;

                return userRequestedLimit == null || results.Count < userRequestedLimit;
            }

            void SetContinuationToken(FeedResponse<dynamic> result)
            {
                if (aggregatesQueried.QueryOptions is WithoutReplayOptionsLibrarySide<T> continueAndTakeOptions
                    && continueAndTakeOptions.MaxTake != null)
                {
                    continueAndTakeOptions.NextContinuationTokenValue = new ContinuationToken(result.ContinuationToken);
                }
            }

            async Task CreateIndexes(List<(string, bool)> fieldName_IsDescending)
            {
                // Retrieve the container's details
                var containerResponse = await this.container.ReadContainerAsync();

                // Add a composite index
                var compositePaths = new Collection<CompositePath>();

                foreach (var valueTuple in fieldName_IsDescending)
                {
                    compositePaths.Add(
                        new CompositePath
                        {
                            Path = $"/{valueTuple.Item1}",
                            Order = valueTuple.Item2 ? CompositePathSortOrder.Descending : CompositePathSortOrder.Ascending
                        });
                }

                containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(compositePaths);

                // Update container with changes
                await this.container.ReplaceContainerAsync(containerResponse.Resource).ConfigureAwait(false);

                long indexTransformationProgress;
                do
                {
                    // retrieve the container's details
                    containerResponse = await this.container.ReadContainerAsync(
                                            new ContainerRequestOptions
                                            {
                                                PopulateQuotaInfo = true
                                            }).ConfigureAwait(false);

                    // retrieve the index transformation progress from the result
                    indexTransformationProgress =
                        long.Parse(containerResponse.Headers["x-ms-documentdb-collection-index-transformation-progress"]);
                }
                while (indexTransformationProgress < 100);
            }
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var query = CreateQueryable<T>();
            var count = await query.Where(d => d.id == aggregateQueriedById.Id).CountAsync().ConfigureAwait(false);
            if (count == 0) return default;

            var result = await this.container.ReadItemAsync<T>(
                             aggregateQueriedById.Id.ToString(),
                             new PartitionKey(Aggregate.PartitionKeyValue)).ConfigureAwait(false);

            var asT = ((T)(dynamic)result)
                /* set eTag */
                .Op(t => t.As<IHaveAnETag>().Etag = result.ETag);
            return asT;
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var preSaveTag = aggregateUpdated.Model.Etag;

            try
            {
                aggregateUpdated.Model.Etag =
                    null; //- clear after copying to access condition, no reason to save and its confusing to see it, this is our eTag property, not the underlying documents

                var result = await this.container.ReplaceItemAsync(
                                 aggregateUpdated.Model,
                                 aggregateUpdated.Model.id.ToString(),
                                 new PartitionKey(Aggregate.PartitionKeyValue),
                                 new ItemRequestOptions
                                 {
                                     IfMatchEtag = preSaveTag //* can be null, note made for reference on conversion to v3 SDK
                                 }).ConfigureAwait(false);

                aggregateUpdated.Model.Etag =
                    result.ETag; //- update etag with value from underlying document and in doing so send it back to caller, see UpdateOperation

                aggregateUpdated.StateOperationCost = result.RequestCharge;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    throw new DBConcurrencyException(
                        $"Etag {preSaveTag} on {aggregateUpdated.Model.GetType().FullName} with id {aggregateUpdated.Model.id} is outdated",
                        e);
                }

                throw;
            }
        }

        async Task IResetData.NonTransactionalReset()
        {
            await new CosmosDbUtilities().ResetDatabase(ConnectionSettings).ConfigureAwait(false);
            ResetClient();
        }

        private void ResetClient()
        {
            CosmosDbUtilities.CreateClient(this.settings, out this.client);
        }
    }
}
