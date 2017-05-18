namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using Models.PureFunctions.Extensions;
    using Newtonsoft.Json;

    public class InMemoryDocumentRepository : IDocumentRepository
    {
        public List<IAggregate> Aggregates { get; set; } = new List<IAggregate>();

        private IEnumerable<T> Clone<T>(IEnumerable<T> toClone) where T : IHaveAUniqueId
        {
            var asJson = JsonConvert.SerializeObject(toClone);
            var cloned = JsonConvert.DeserializeObject<IEnumerable<T>>(asJson);
            return cloned;
        }

        #region IDocumentRepository Members

        public Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            Aggregates.Add(aggregateAdded.Model);

            return Task.FromResult(aggregateAdded.Model);
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            return Clone(Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>()).AsQueryable();
        }

        public Task DeleteHardAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var aggregate = Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>().Single(a => a.id == aggregateHardDeleted.Model.id);

            Aggregates.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            return Task.FromResult(aggregate);
        }

        public Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new()
        {
            var aggregate = Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>().Single(a => a.id == aggregateSoftDeleted.Model.id);

            var now = DateTime.UtcNow;
            aggregate.Active = false;
            aggregate.Modified = now;
            aggregate.ModifiedAsMillisecondsEpochTime = now.ConvertToMillisecondsEpochTime();
            
            return Task.FromResult(aggregate);
        }

        public void Dispose()
        {
            Aggregates.Clear();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var cloned = aggregatesQueried.Query.ToList().AsEnumerable();

            return Task.FromResult(cloned);
        }

        public Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            return Task.FromResult(Aggregates.Exists(a => a.id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            var aggregate = Aggregates.Where(x => x.schema == typeof(T).FullName).Cast<T>().Single(a => a.id == aggregateQueriedById.Id);

            return Task.FromResult(aggregate);
        }

        public Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            var queryable = Aggregates.AsQueryable().Where(x => x.id == aggregateQueriedById.Id);

            var document = queryable.ToList().Single();

            return Task.FromResult<dynamic>(document);
        }

        public Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var toUpdate = Aggregates.Single(x => x.id == aggregateUpdated.Model.id);

            aggregateUpdated.Model.CopyProperties(toUpdate);

            return Task.FromResult((T) toUpdate);
        }

        #endregion
    }
}