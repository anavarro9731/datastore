namespace DataStore.Providers.CosmosDb
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;
    using DataStore.Models.PartitionKeys;
    using DataStore.Models.PureFunctions;
    using DataStore.Models.PureFunctions.Extensions;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Newtonsoft.Json.Linq;

    #endregion

    public class CosmosDbRepository : IDocumentRepository, IResetData
    {
        private readonly Container container;

        private readonly CosmosSettings settings;

        private CosmosClient client;

        public CosmosDbRepository(CosmosSettings settings)
        {
            settings.ClientOptions = settings.ClientOptions ?? new CosmosClientOptions();
            settings.ClientOptions.Serializer = new CosmosJsonDotNetSerializer();
            CosmosDbUtilities.CreateClient(settings, out this.client);
            this.container = this.client.GetContainer(settings.DatabaseName, settings.ContainerName);
            this.settings = settings;
        }

        public IDatabaseSettings ConnectionSettings => this.settings;

        public bool UseHierarchicalPartitionKeys => this.settings.UseHierarchicalPartitionKeys;

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded?.Model == null)
            {
                throw new ArgumentNullException(nameof(aggregateAdded));
            }

            //* will get partition key from model
            var result = await this.container.CreateItemAsync(aggregateAdded.Model, aggregateAdded.Model.PartitionKeys.ToCosmosPartitionKey(UseHierarchicalPartitionKeys)).ConfigureAwait(false);

              aggregateAdded.Model.Etag = result.ETag; //* update it

            aggregateAdded.StateOperationCost = result.RequestCharge;
        }

        public async Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateQueryable<T>(aggregatesCounted.QueryOptions);

            if (aggregatesCounted.Predicate != default) query = query.Where(aggregatesCounted.Predicate);
            var count = await query.CountAsync().ConfigureAwait(false);

            return count;
        }

        public IQueryable<T> CreateQueryable<T>(IOptionsLibrarySide queryOptions) where T : class, IAggregate, new()
        {
            var schema = typeof(T).FullName;
            Expression<Func<T,bool>> predicate = i => i.Schema == schema;
            
            var key = PartitionKeyHelpers.GetKeysForLinqQuery<T>(this.UseHierarchicalPartitionKeys, queryOptions.As<IPartitionKeyOptionsLibrarySide>());
            
            var fanout = key.IsEmpty();
            
            if (!fanout)  predicate = predicate.And(key.ToPredicate<T>(UseHierarchicalPartitionKeys));
            
            var query = this.container.GetItemLinqQueryable<T>().Where(predicate);
            
            return query;
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var result = await this.container.DeleteItemAsync<T>(
                             aggregateHardDeleted.Model.id.ToString(),
                             aggregateHardDeleted.Model.PartitionKeys.ToCosmosPartitionKey(UseHierarchicalPartitionKeys)).ConfigureAwait(false);

            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
            aggregateHardDeleted.Model.Etag = "item was deleted";
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried) where T : class, IAggregate, new()
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

                /* it would be quite a lot easier to just use the AggregatesQueried.Query.ToIterator()
                but that would give you a typed iterator, and no way to get back an untyped feed response
                which you must have in order to at the updated ETag and copy it back to our customer eTag property.
                So for now we convert the CosmosLINQQueryable into a generic one. */

                using (var setIterator = this.container.GetItemQueryIterator<T>(
                           aggregatesQueried.Query.ToQueryDefinition(),
                           (aggregatesQueried.QueryOptions as WithoutReplayOptionsLibrarySide<T>)?.CurrentContinuationToken?.ToString(),
                           new QueryRequestOptions
                           {
                               MaxItemCount = (aggregatesQueried.QueryOptions as WithoutReplayOptionsLibrarySide<T>)?.MaxTake,
                               ConsistencyLevel = ConsistencyLevel.Session //should be the default anyway
                           }))
                {
                    while (HaveLessRecordsThanUserRequested() && setIterator.HasMoreResults)
                    {
                        var feedResponseEnumerable = await setIterator.ReadNextAsync().ConfigureAwait(false);

                        aggregatesQueried.StateOperationCost += feedResponseEnumerable.RequestCharge;

                        var ruLimit = aggregatesQueried.QueryOptions.As<IPerformanceOptionsLibrarySide>()?.BypassRULimit ?? false
                                          ? short.MaxValue
                                          : this.settings.MaxQueryCostInRus; 
                        
                        if (aggregatesQueried.StateOperationCost > ruLimit)
                        {
                            throw new Exception($"Query cost of {aggregatesQueried.StateOperationCost} exceeds limit of {this.settings.MaxQueryCostInRus} RUs, abandoning");
                        }

                        results.AddRange(feedResponseEnumerable);

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

            void SetContinuationToken(FeedResponse<T> result)
            {
                if (aggregatesQueried.QueryOptions is WithoutReplayOptionsLibrarySide<T> continueAndTakeOptions && continueAndTakeOptions.MaxTake != null)
                {
                    continueAndTakeOptions.NextContinuationTokenValue = new ContinuationToken(result.ContinuationToken);
                }
            }

            async Task CreateIndexes(List<(string, bool)> fieldName_IsDescending)
            {
                // Retrieve the container's details
                var containerResponse = await this.container.ReadContainerAsync().ConfigureAwait(false);

                // Add a composite index
                var newCompositeIndex = new Collection<CompositePath>();

                foreach (var valueTuple in fieldName_IsDescending)
                    newCompositeIndex.Add(
                        new CompositePath
                        {
                            Path = $"/{valueTuple.Item1}", Order = valueTuple.Item2 ? CompositePathSortOrder.Descending : CompositePathSortOrder.Ascending
                        });

                var exists = containerResponse.Resource.IndexingPolicy.CompositeIndexes.Any(
                    existingCompositeIndex =>
                        {
                        //* this existing index doesn't match the new one
                        if (newCompositeIndex.Count != existingCompositeIndex.Count) return false;

                        for (var i = 0; i < existingCompositeIndex.Count; i++)
                            //* this existing index doesn't match the new one
                            if (existingCompositeIndex[i].Path != newCompositeIndex[i].Path || existingCompositeIndex[i].Order != newCompositeIndex[i].Order)
                            {
                                return false;
                            }

                        //* if we have the same number of fields, and all the paths and orders match, then it exists
                        return true;
                        });
                if (!exists)
                {
                    containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(newCompositeIndex);

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
                        indexTransformationProgress = long.Parse(containerResponse.Headers["x-ms-documentdb-collection-index-transformation-progress"]);
                    }
                    while (indexTransformationProgress < 100);
                }
            }
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadByIdOperation aggregateQueriedByIdOperation) where T : class, IAggregate, new()
        {
            ItemResponse<T> result = null;
            try
            {
                var key = PartitionKeyHelpers.GetKeysForExistingItemFromId<T>(UseHierarchicalPartitionKeys, aggregateQueriedByIdOperation.Id, aggregateQueriedByIdOperation.QueryOptions as IPartitionKeyOptionsLibrarySide);

                result = await this.container.ReadItemAsync<T>(aggregateQueriedByIdOperation.Id.ToString(), key.ToCosmosPartitionKey(UseHierarchicalPartitionKeys)).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound || (ex.StatusCode == HttpStatusCode.BadRequest && ex.Message.Contains("Partition")))
            {
            }

            if (result == null) return default;

            var asT = ((T)(dynamic)result)
                /* set eTag */
                .Op(t => t.As<IHaveAnETag>().Etag = result.ETag);

            if (asT.PartitionKey == "shared" && (asT.PartitionKeys == null || asT.PartitionKeys.IsEmpty()))
            {
                //* this is an aggregate created before partition key support was introduced
                asT.PartitionKeys = new HierarchicalPartitionKey()
                {
                    Key1 = "sh", Key2 = "ar", Key3 = "ed"
                };
            }
            
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
                                 aggregateUpdated.Model.PartitionKeys.ToCosmosPartitionKey(UseHierarchicalPartitionKeys),
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
            CosmosDbUtilities.CreateClient(this.settings, out this.client);
        }
    }
}