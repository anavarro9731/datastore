using PalmTree.Infrastructure.Interfaces;

namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Messages.Events;

    // Not sure if eventreplay makes sense in this class, needs review currently its not implemented.
    // It's also questionable what happens to events subsquent to a hard-delete in a session, how does it error?

    // All methods return the version of the object before it was deleted, for soft delete this is probably
    // a bit confusing, but trying to mark them in this class raises issues with duplication of logic in 
    // the documentRepository and matching the timestamps. Needs review.

    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IEventAggregator eventAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        #region IDataStoreDeleteCapabilities Members

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            var result = await eventAggregator.Store(new AggregateQueriedById(nameof(DeleteHardById), id, typeof(T)))
                .ForwardToAsync(DsConnection.GetItemAsync<T>);

            eventAggregator.Store(new AggregateHardDeleted<T>(nameof(DeleteHardById), result, DsConnection));

            return result;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteHardWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DsConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                eventAggregator.Store(new AggregateHardDeleted<T>(nameof(DeleteHardWhere), dataObject, DsConnection));

            return dataObjects;
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : IAggregate
        {
            var result = await eventAggregator.Store(new AggregateQueriedById(nameof(DeleteSoftById), id, typeof(T)))
                .ForwardToAsync(DsConnection.GetItemAsync<T>);

            eventAggregator.Store(new AggregateSoftDeleted<T>(nameof(DeleteSoftById), result, DsConnection));

            return result;
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteSoftWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DsConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                eventAggregator.Store(new AggregateSoftDeleted<T>(nameof(DeleteSoftWhere), dataObject, DsConnection));

            return dataObjects;
        }

        #endregion
    }
}