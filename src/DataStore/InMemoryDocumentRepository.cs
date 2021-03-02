namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class InMemoryDocumentRepository : IDocumentRepository, IResetData
    {
        public List<IAggregate> Aggregates { get; set; } = new List<IAggregate>();

        public IDatabaseSettings ConnectionSettings => new Settings(this);

        public Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            var toAdd = aggregateAdded.Model;

            //- fake eTag change
            toAdd.Etag = Guid.NewGuid().ToString();

            Aggregates.Add(toAdd);

            return Task.CompletedTask;
        }

        public Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateQueryable<T>();

            var count = aggregatesCounted.Predicate == null ? query.Count() : query.Count(aggregatesCounted.Predicate);

            return Task.FromResult(count);
        }

        public IQueryable<T> CreateQueryable<T>(object queryOptions = null) where T : class, IAggregate, new()
        {
            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Aggregates.Where(x => x.Schema == typeof(T).FullName).Cast<T>().Clone().AsQueryable();
        }

        public Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            Aggregates.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            aggregateHardDeleted.Model.Etag = "item was deleted";

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Aggregates.Clear();
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

        public Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            return Task.FromResult(Aggregates.Exists(a => a.id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var aggregate = Aggregates.Where(x => x.Schema == typeof(T).FullName).Cast<T>()
                                      .SingleOrDefault(a => a.id == aggregateQueriedById.Id);

            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Task.FromResult(aggregate?.Clone());
        }

        public Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var updatedRecord = aggregateUpdated.Model;
            var existingRecord = Aggregates.Single(x => x.id == updatedRecord.id);

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
            Aggregates.Clear();

            return Task.CompletedTask;
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

            public IDocumentRepository CreateRepository() => this.repository;
        }
    }
}