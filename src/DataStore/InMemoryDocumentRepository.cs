namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    #endregion

    public class InMemoryDocumentRepository : IDocumentRepository, IResetData
    {
        public InMemoryDocumentRepository(bool useHierarchicalPartitionKeys = false)
        {
            UseHierarchicalPartitionKeys = useHierarchicalPartitionKeys;
        }

        
        public Dictionary<HierarchicalPartitionKey, List<IAggregate>> AggregatesByLogicalPartition { get; set; } = new Dictionary<HierarchicalPartitionKey, List<IAggregate>>();

        public IDatabaseSettings ConnectionSettings => new Settings(this);

        public bool UseHierarchicalPartitionKeys { get; }

        public Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            var toAdd = aggregateAdded.Model;

            //- fake eTag change
            toAdd.Etag = Guid.NewGuid().ToString();

            var keys = aggregateAdded.Model.PartitionKeys;
            if (AggregatesByLogicalPartition.ContainsKey(keys))
            {
                AggregatesByLogicalPartition[keys].Add(aggregateAdded.Model);
            }
            else
            {
                AggregatesByLogicalPartition.Add(
                    keys,
                    new List<IAggregate>
                    {
                        aggregateAdded.Model
                    });
            }

            return Task.CompletedTask;
        }

        public Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateQueryable<T>(aggregatesCounted.QueryOptions);

            var count = aggregatesCounted.Predicate == null ? query.Count() : query.Count(aggregatesCounted.Predicate);

            return Task.FromResult(count);
        }

        public IQueryable<T> CreateQueryable<T>(IQueryOptions queryOptions = null) where T : class, IAggregate, new()
        {
            //* this will limit the range of the queryable to the partitions which match the partition keys constructable from the supplied data
            var keys = PartitionKeyHelpers.GetKeysForLinqQuery<T>(this.UseHierarchicalPartitionKeys, queryOptions.As<IPartitionKeyOptions>());

            //* this will return the partitions to search, if the PK is empty and mode synthetic, then this will be all partitions by default
            var partitions = FindPartitionsFromKeys<T>(keys);

            //* find all aggregates in all matching partitions where the type matches
            var aggregates = AggregatesByLogicalPartition.Where(x => partitions.Contains(x.Key)).SelectMany(x => x.Value).Where(x => x.Schema == typeof(T).FullName)
                                                         .Cast<T>();

            //* clone otherwise its to easy to change the referenced object in test code affecting results
            return aggregates.Clone().AsQueryable();
        }

        public Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var keys = aggregateHardDeleted.Model.PartitionKeys;
            Guard.Against(AggregatesByLogicalPartition.ContainsKey(keys) == false, "Cannot find the partition(s) containing the requested operation");

            var partitions = AggregatesByLogicalPartition[keys];
            partitions.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            aggregateHardDeleted.Model.Etag = "item was deleted";

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            AggregatesByLogicalPartition.Clear();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried) where T : class, IAggregate, new()
        {
            var results = new List<T>();
            
            
            if (aggregatesQueried.QueryOptions is WithoutReplayOptionsLibrarySide<T> orderByOptions)
            {
                aggregatesQueried.Query = orderByOptions.AddOrderBy(aggregatesQueried.Query);
            }

            if (aggregatesQueried.QueryOptions is WithoutReplayOptionsLibrarySide<T> skipAndTakeOptions)
            {
                {
                    AddSkipIfExists(out var skip);

                    AddTakeIfExists(out var take);

                    if (take < int.MaxValue)
                    {
                        skipAndTakeOptions.NextContinuationTokenValue = new ContinuationToken(skip + take);
                    }
                }

                void AddSkipIfExists(out int skip)
                {
                    skip = skipAndTakeOptions.CurrentContinuationToken?.ToInt() ?? 0;
                    if (skip > 0)
                    {
                        aggregatesQueried.Query = aggregatesQueried.Query.Skip(skip);
                    }
                }

                void AddTakeIfExists(out int take)
                {
                    take = skipAndTakeOptions.MaxTake ?? int.MaxValue;
                    if (take < int.MaxValue)
                    {
                        aggregatesQueried.Query = aggregatesQueried.Query.Take(take);
                    }
                }
            }

            //.. execute query
            
            results.AddRange(aggregatesQueried.Query.ToList());
            //* no need to set the eTag value, it will already be correct unlike other providers where it has to be mapped from an underlying document

            /* clone otherwise its to easy to change the referenced
             object in test code affecting results */
            var result = results.Clone().AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadByIdOperation aggregateQueriedByIdOperation) where T : class, IAggregate, new()
        {
            //* with getbyid we will expect exact and full partitions 
            var keys = PartitionKeyHelpers.GetKeysForExistingItemFromId<T>(this.UseHierarchicalPartitionKeys, 
                aggregateQueriedByIdOperation.Id, aggregateQueriedByIdOperation.QueryOptions.As<IPartitionKeyOptions>());

            if (AggregatesByLogicalPartition.ContainsKey(keys))
            {
                var partitions = AggregatesByLogicalPartition[keys];

                var aggregate = partitions.Where(x => x.Schema == typeof(T).FullName).Cast<T>().SingleOrDefault(a => a.id == aggregateQueriedByIdOperation.Id);

                //clone otherwise its to easy to change the referenced object in test code affecting results
                return Task.FromResult(aggregate?.Clone());
            }

            return Task.FromResult((T)null);
        }

        public Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var updatedRecord = aggregateUpdated.Model;

            var keys = aggregateUpdated.Model.PartitionKeys;
            Guard.Against(AggregatesByLogicalPartition.ContainsKey(keys) == false, "Cannot find the partition(s) containing the requested operation");

            var partitions = AggregatesByLogicalPartition[keys];

            var existingRecord = partitions.Single(x => x.id == updatedRecord.id);

            var optimisticConcurrencyDisabled = updatedRecord.Etag == null;
            if (updatedRecord.Etag != existingRecord.Etag && !optimisticConcurrencyDisabled)
            {
                throw new DBConcurrencyException(
                    $"Etag {aggregateUpdated.Model.Etag} on {aggregateUpdated.Model.GetType().FullName} with id {aggregateUpdated.Model.id} is outdated");
            }

            updatedRecord.CopyPropertiesTo(existingRecord);

            //- fake eTag update, by updating aggregateUpdated.Model this is sent back to the client (see UpdateOperation class)
            updatedRecord.Etag = Guid.NewGuid().ToString();
            //* locally we have no underlying Document object so we save the updated Tag directly on the in-memory object
            existingRecord.Etag = updatedRecord.Etag;

            return Task.CompletedTask;
        }

        Task IResetData.NonTransactionalReset()
        {
            AggregatesByLogicalPartition.Clear();

            return Task.CompletedTask;
        }

        private List<HierarchicalPartitionKey> FindPartitionsFromKeys<T>(HierarchicalPartitionKey key) where T : class, IAggregate, new()
        {
            switch (AggregatesByLogicalPartition.ContainsKey(key))
            {
                case false when UseHierarchicalPartitionKeys:
                    {
                        //* search for partial fan out
                        
                            var matchingPartitions = AggregatesByLogicalPartition.Where(x => x.Key.IsAssignableToReducedKey(key)).ToList();
                            if (matchingPartitions.Any())
                            {
                                return matchingPartitions.Select(x => x.Key).ToList();
                            }
                        

                        //* nothing found 
                        return new List<HierarchicalPartitionKey>();
                    }
                case false when !UseHierarchicalPartitionKeys:
                    {
                        //* full fan out no key data supplied
                        var fanOut = key.IsEmpty();
                        return fanOut ? AggregatesByLogicalPartition.Keys.ToList() : new List<HierarchicalPartitionKey>();
                    }
                default:
                    return new List<HierarchicalPartitionKey>
                    {
                        key
                    };
            }
        }

        


        /* should return the same backing store as any other instances
         so that it mirrors the capability of persistent-state backed providers.
         This will show up as subtle errors otherwise in unit-testing when
         the ConnectionSettings are used to create a simultaneous session. */
        public class Settings : IDatabaseSettings
        {
            private readonly IDocumentRepository repository;

            public Settings(IDocumentRepository repository)
            {
                this.repository = repository;
            }

            public IDocumentRepository CreateRepository()
            {
                return this.repository;
            }
        }
    }

    public static class Ext
    {
        public static bool IsAssignableToReducedKey(this HierarchicalPartitionKey key, HierarchicalPartitionKey reducedKey)
        {
            return reducedKey.Key1 == key.Key1 && 
                   (string.IsNullOrWhiteSpace(reducedKey.Key2) || reducedKey.Key2 == key.Key2) &&
                   (string.IsNullOrWhiteSpace(reducedKey.Key3) || reducedKey.Key3 == key.Key3);
        }
    }
}