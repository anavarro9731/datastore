namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using Messages.Events;

    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        #region IDataStoreDeleteCapabilities Members

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(DeleteHardById), id, typeof(T)))
                .ForwardToAsync(DbConnection.GetItemAsync<T>);

            return await _eventAggregator.Store(new AggregateHardDeleted<T>(result)).ForwardToAsync(DbConnection.DeleteHardAsync);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteHardWhere), DbConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DbConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
            {
                await _eventAggregator.Store(new AggregateHardDeleted<T>(dataObject)).ForwardToAsync(DbConnection.DeleteHardAsync);
            }

            return dataObjects;
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : IAggregate
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(DeleteSoftById), id, typeof(T)))
                .ForwardToAsync(DbConnection.GetItemAsync<T>);

            return await _eventAggregator.Store(new AggregateSoftDeleted<T>(result)).ForwardToAsync(DbConnection.DeleteSoftAsync);
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteSoftWhere), DbConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DbConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
            {
                dataObject.SoftDelete();
                await _eventAggregator.Store(new AggregateSoftDeleted<T>(dataObject)).ForwardToAsync(DbConnection.DeleteSoftAsync);
            }

            return dataObjects;
        }

        #endregion
    }
}