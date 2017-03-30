using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.Events;
using DataStore.Models.PureFunctions.Extensions;
using Newtonsoft.Json;
using ServiceApi.Interfaces.LowLevel;

namespace DataStore
{
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

        public Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate
        {
            Aggregates.Add(aggregateAdded.Model);

            return Task.FromResult(aggregateAdded.Model);
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            return Clone(Aggregates.OfType<T>()).AsQueryable();
        }

        public Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate
        {
            var aggregate = Aggregates.OfType<T>().Single(a => a.Id == aggregateHardDeleted.Model.Id);

            Aggregates.RemoveAll(a => a.Id == aggregateHardDeleted.Model.Id);

            return Task.FromResult(aggregate);
        }

        public Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate
        {
            var aggregate = Aggregates.OfType<T>().Single(a => a.Id == aggregateSoftDeleted.Model.Id);

            (aggregate as dynamic).Active = false;

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
            return Task.FromResult(Aggregates.Exists(a => a.Id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            var aggregate = Aggregates.OfType<T>().Single(a => a.Id == aggregateQueriedById.Id);

            return Task.FromResult(aggregate);
        }

        public Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            var queryable = Aggregates.AsQueryable().Where(x => x.Id == aggregateQueriedById.Id);

            var document = queryable.ToList().Single();
            
            return Task.FromResult<dynamic>(document);
        }

        public Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate
        {
            var toUpdate = Aggregates.Single(x => x.Id == aggregateUpdated.Model.Id);

            aggregateUpdated.Model.CopyProperties(toUpdate);

            return Task.FromResult((T) toUpdate);
        }

        #endregion
    }
}