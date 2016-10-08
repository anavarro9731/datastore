﻿namespace DataStore.DataAccess.Impl.DocumentDb
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.PureFunctions.PureFunctions.Extensions;
    using Interfaces;
    using Messages.Events;
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    public class InMemoryDocumentRepository : IDocumentRepository
    {
        public List<IHaveAUniqueId> Aggregates { get; set; } = new List<IHaveAUniqueId>();

        #region IDocumentRepository Members

        public Task<T> AddAsync<T>(AggregateAdded<T> aggregateAdded) where T : IHaveAUniqueId
        {
            Aggregates.Add(aggregateAdded.Model);

            return Task.FromResult(aggregateAdded.Model);
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            return Aggregates.Where(x => x is T).Cast<T>().AsQueryable();
        }

        public Task<T> DeleteHardAsync<T>(AggregateHardDeleted<T> aggregateHardDeleted) where T : IHaveAUniqueId
        {
            var aggregate = Aggregates.Single(a => a.id == aggregateHardDeleted.Model.id);

            Aggregates.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            return Task.FromResult((T) aggregate);
        }

        public Task<T> DeleteSoftAsync<T>(AggregateSoftDeleted<T> aggregateSoftDeleted) where T : IHaveAUniqueId
        {
            var aggregate = Aggregates.Single(a => a.id == aggregateSoftDeleted.Model.id);

            return Task.FromResult((T) aggregate);
        }

        public void Dispose()
        {
            Aggregates.Clear();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(AggregatesQueried<T> aggregatesQueried) where T : IHaveAUniqueId
        {
            return Task.FromResult(aggregatesQueried.Query.ToList().AsEnumerable());
        }

        public Task<bool> Exists(AggregateQueriedById aggregateQueriedById)
        {
            return Task.FromResult(Aggregates.Exists(a => a.id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(AggregateQueriedById aggregateQueriedById) where T : IHaveAUniqueId
        {
            return Task.FromResult(Aggregates.Cast<T>().Single(a => a.id == aggregateQueriedById.Id));
        }

        public Task<Document> GetItemAsync(AggregateQueriedById aggregateQueriedById)
        {
            var queryable = Aggregates.AsQueryable().Where(x => x.id == aggregateQueriedById.Id);

            var d = new Document();

            var json = JsonConvert.SerializeObject(queryable.ToList().Single());

            d.LoadFrom(new JsonTextReader(new StringReader(json)));

            return Task.FromResult(d);
        }

        public Task<T> UpdateAsync<T>(AggregateUpdated<T> aggregateUpdated) where T : IHaveAUniqueId
        {
            return UpdateAsync(aggregateUpdated.Model);
        }

        #endregion

        private Task<T> UpdateAsync<T>(T item) where T : IHaveAUniqueId
        {
            var toUpdate = Aggregates.Single(x => x.id == item.id);

            item.CopyProperties(toUpdate);

            return Task.FromResult((T) toUpdate);
        }
    }
}