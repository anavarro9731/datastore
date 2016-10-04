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
        private readonly IEventAggregator eventAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            var result = await DbConnection.GetItemAsync<T>(id);
            return await eventAggregator.Store(new AggregateHardDeleted<T>(result)).ForwardToAsync(DbConnection.DeleteHardAsync);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await DbConnection.ExecuteQuery(DbConnection.CreateDocumentQuery<T>().Where(predicate));

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
            {
                await eventAggregator.Store(new AggregateHardDeleted<T>(dataObject)).ForwardToAsync(DbConnection.DeleteHardAsync);
            }

            return dataObjects;
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T: IAggregate
        {
            var result = await DbConnection.GetItemAsync<T>(id);

            return await eventAggregator.Store(new AggregateSoftDeleted<T>(result)).ForwardToAsync(DbConnection.DeleteSoftAsync);
            
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            var objects = await DbConnection.ExecuteQuery(DbConnection.CreateDocumentQuery<T>().Where(predicate));

            var dataObjects = objects.AsEnumerable();
            foreach (var dataObject in dataObjects)
            {
                dataObject.SoftDelete();
                await eventAggregator.Store(new AggregateSoftDeleted<T>(dataObject)).ForwardToAsync(DbConnection.DeleteSoftAsync);
            }

            return dataObjects;
        }
    }
}