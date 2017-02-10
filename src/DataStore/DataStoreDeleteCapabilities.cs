namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Messages.Events;

    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        #region IDataStoreDeleteCapabilities Members

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(DeleteHardById), id, typeof(T)))
                .ForwardToAsync(DsConnection.GetItemAsync<T>);

            _eventAggregator.Store(new AggregateHardDeleted<T>(nameof(DeleteHardById), result, DsConnection));

            return result;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteHardWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DsConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                _eventAggregator.Store(new AggregateHardDeleted<T>(nameof(DeleteHardWhere), dataObject, DsConnection));

            return dataObjects;
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : IAggregate
        {
            var result = await _eventAggregator.Store(new AggregateQueriedById(nameof(DeleteSoftById), id, typeof(T)))
                .ForwardToAsync(DsConnection.GetItemAsync<T>);

            _eventAggregator.Store(new AggregateSoftDeleted<T>(nameof(DeleteSoftById), result, DsConnection));

            return result;
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(DeleteSoftWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DsConnection.ExecuteQuery);

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
                _eventAggregator.Store(new AggregateSoftDeleted<T>(nameof(DeleteSoftWhere), dataObject, DsConnection));

            return dataObjects;
        }

        #endregion
    }
}