namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class InMemoryDocumentRepository : IDocumentRepository, IResetData
    {
        public List<IAggregate> Aggregates { get; set; } = new List<IAggregate>();

        public Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            Aggregates.Add(aggregateAdded.Model);

            return Task.CompletedTask;
        }

        public Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var query = CreateDocumentQuery<T>();

            var count = aggregatesCounted.Predicate == null ? query.Count() : query.Count(aggregatesCounted.Predicate);

            return Task.FromResult(count);
        }

        public IQueryable<T> CreateDocumentQuery<T>(IQueryOptions<T> queryOptions = null) where T : class, IEntity, new()
        {
            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Aggregates.Where(x => x.Schema == typeof(T).FullName).Cast<T>().Clone().AsQueryable();
        }

        public Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            Aggregates.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Aggregates.Clear();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var results = new List<T>();

            if (aggregatesQueried.QueryOptions is IOrderBy<T> orderByOptions)
            {
                aggregatesQueried.Query = orderByOptions.AddOrderBy(aggregatesQueried.Query);
            }

            if (aggregatesQueried.QueryOptions is IContinueAndTake<T> skipAndTakeOptions)
            {
                {
                    AddSkipIfExists(out int skip);

                    AddTakeIfExists(out int take);

                    if (take < int.MaxValue)
                    {
                        skipAndTakeOptions.NextContinuationToken = new ContinuationToken(skip + take);
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
            var aggregate = Aggregates.Where(x => x.Schema == typeof(T).FullName).Cast<T>().SingleOrDefault(a => a.id == aggregateQueriedById.Id);

            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Task.FromResult(aggregate?.Clone());
        }

        public Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var toUpdate = Aggregates.Single(x => x.id == aggregateUpdated.Model.id);

            aggregateUpdated.Model.CopyProperties(toUpdate);

            return Task.CompletedTask;
        }

        Task IResetData.NonTransactionalReset()
        {
            Aggregates.Clear();

            return Task.CompletedTask;
        }
    }
}