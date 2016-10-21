using DataStore.DataAccess.Interfaces.Events;
using DataStore.DataAccess.Models.Messages.Events;
using DataStore.Infrastructure.PureFunctions.PureFunctions.Extensions;
using FluentValidation.TestHelper;

namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using Microsoft.Azure.Documents;

    public class DataStoreQueryCapabilities : IDataStoreQueryCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreQueryCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            this.DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<bool> Exists(Guid id)
        {
            if (id == Guid.Empty) return false;
            return await _eventAggregator.Store(new AggregateQueriedById(nameof(Exists), id)).ForwardToAsync(DbConnection.Exists);
        }

        // get a filtered list of the models from set of DataObjects
        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate
        {
            var results = await ReadInternal(queryableExtension);

            return ApplyAggregateEvents(results, false);
        }
                
        // get a filtered list of the models from a set of active DataObjects
        public async Task<IEnumerable<T>> ReadActive<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> activeOnlyQueryableExtension = (q) =>
                {
                    if (queryableExtension != null)
                    {
                        q = queryableExtension(q);
                    }

                    q = q.Where(a => a.Active);

                    return q;
                };

            var results = await ReadInternal(activeOnlyQueryableExtension);

            return ApplyAggregateEvents(results, true);
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = (q) => q.Where(a => a.id == modelId && a.Active);
            var results = await ReadInternal(queryableExtension);

            return ApplyAggregateEvents(results, true).Single();
        }

        // get a filtered list of the models from  a set of DataObjects
        public async Task<Document> ReadById(Guid modelId)
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(ReadById), modelId)).ForwardToAsync(DbConnection.GetItemAsync);
            return result;
        }

        private async Task<IEnumerable<T>> ReadInternal<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension) where T : IAggregate
        {
            var queryable = this.DbConnection.CreateDocumentQuery<T>();
            if (queryableExtension != null)
            {
                queryable = queryableExtension(queryable);
            }

            var results = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(ReadInternal), queryable)).ForwardToAsync(DbConnection.ExecuteQuery);
            return results;
        }        

        private List<T> ApplyAggregateEvents<T>(IEnumerable<T> results, bool isReadActive) where T : IAggregate
        {
            var modifiedResults = results.ToList();
            var uncommittedEvents = _eventAggregator.Events.OrderBy(e => e.OccurredAt).OfType<IDataStoreWriteEvent<T>>().Where(e => !e.Committed);

            foreach (var eventAggregatorEvent in uncommittedEvents)
            {
                ApplyEvent(modifiedResults, eventAggregatorEvent, isReadActive);
            }

            return modifiedResults;
        }

        private static void ApplyEvent<T>(IList<T> results, IDataStoreWriteEvent<T> eventAggregatorEvent, bool isReadActive) where T : IAggregate
        {
            if (eventAggregatorEvent is AggregateAdded<T>)
            {
                results.Add(eventAggregatorEvent.Model);
            }

            if (eventAggregatorEvent is AggregateUpdated<T>)
            {
                var itemToUpdate = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                eventAggregatorEvent.Model.CopyProperties(itemToUpdate);
            }

            if (eventAggregatorEvent is AggregateSoftDeleted<T>)
            {
                if (isReadActive)
                {
                    var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                    results.Remove(itemToRemove);
                }
            }

            if (eventAggregatorEvent is AggregateHardDeleted<T>)
            {
                var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                results.Remove(itemToRemove);
            }
        }
    }
}