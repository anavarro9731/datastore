namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;
    using DataAccess.Messages.Events;

    using Infrastructure.HandlerServiceInterfaces;

    internal class DataStoreUpdateCapabilities : IDataStoreUpdateCapabilities
    {
        private readonly IEventAggregator eventAggregator;

        public DataStoreUpdateCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : IAggregate
        {
            var results = await UpdateWhere(o => o.id == id, action, overwriteReadOnly);

            return results.Single();
        }

        // .. update by Id; get values from any instance
        public async Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(Guid id, T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            var results =
                await
                UpdateWhere<T>(o => o.id == id, model => { model.UpdateFromAnotherObject(src, nameof(model.id)); }, overwriteReadOnly);

            return results.Single();
        }

        // .. update using Id; get values from another instance
        public async Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            return await UpdateByIdUsingValuesFromAnotherInstance(src.id, src, overwriteReadOnly);
        }


        
        // update a DataObject selected with a singular predicate
        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : IAggregate
        {
            var objects = await DbConnection.ExecuteQuery(DbConnection.CreateDocumentQuery<T>().Where(predicate));

            var dataObjects = objects.AsEnumerable();
            if (dataObjects.Any(dataObject => dataObject.ReadOnly && !overwriteReadOnly))
            {
                throw new ApplicationException("Cannot update read-only items");
            }

            foreach (var dataObject in dataObjects)
            {
                action(dataObject);
                await eventAggregator.Store(new AggregateUpdated<T>(dataObject)).ForwardToAsync(DbConnection.UpdateAsync);
            }

            return dataObjects;
        }
    }
}