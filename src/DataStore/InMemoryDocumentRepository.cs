namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class InMemoryDocumentRepository : IDocumentRepository
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

        public IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new()
        {
            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>().Clone().AsQueryable();
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
            if (aggregatesQueried.QueryOptions is QueryOptions options)
            {
                if (options.Skip > 0)
                {
                    aggregatesQueried.Query = aggregatesQueried.Query.Skip(options.Skip);
                }

                if (options.Take > 0)
                {
                    aggregatesQueried.Query = aggregatesQueried.Query.Take(options.Take);
                }
            }

            //clone otherwise its to easy to change the referenced object in test code affecting results
            var result = aggregatesQueried.Query.ToList().Clone().AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            return Task.FromResult(Aggregates.Exists(a => a.id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var aggregate = Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>().SingleOrDefault(a => a.id == aggregateQueriedById.Id);

            //clone otherwise its to easy to change the referenced object in test code affecting results
            return Task.FromResult(aggregate?.Clone());
        }

        public Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var toUpdate = Aggregates.Single(x => x.id == aggregateUpdated.Model.id);

            aggregateUpdated.Model.CopyProperties(toUpdate);

            return Task.CompletedTask;
        }

        public class QueryOptions : IQueryOptions
        {
            private QueryOptions(int skip, int take)
            {
                Skip = skip;
                Take = take;
            }

            public int Skip { get; set; }

            public int Take { get; set; }

            public static QueryOptions Create(int skip, int take)
            {
                return new QueryOptions(skip, take);
            }
        }
    }
}