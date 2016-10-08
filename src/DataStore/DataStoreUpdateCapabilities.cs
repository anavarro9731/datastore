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

    internal class DataStoreUpdateCapabilities : IDataStoreUpdateCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreUpdateCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DbConnection = dataStoreConnection;
        }

        private IDocumentRepository DbConnection { get; }

        #region IDataStoreUpdateCapabilities Members

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : IAggregate
        {
            var results = await UpdateWhere(o => o.id == id, action, overwriteReadOnly);

            return results.Single();
        }

        // .. update using Id; get values from another instance
        public async Task<T> Update<T>(T src, bool overwriteReadOnly = true)
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
            var objects = await _eventAggregator.Store(new AggregatesQueried<T>(nameof(UpdateWhere), DbConnection.CreateDocumentQuery<T>().Where(predicate)))
                .ForwardToAsync(DbConnection.ExecuteQuery);
            
            var dataObjects = objects.AsEnumerable();
            if (dataObjects.Any(dataObject => dataObject.ReadOnly && !overwriteReadOnly))
            {
                throw new ApplicationException("Cannot update read-only items");
            }

            foreach (var dataObject in dataObjects)
            {
                action(dataObject);
                await _eventAggregator.Store(new AggregateUpdated<T>(dataObject)).ForwardToAsync(DbConnection.UpdateAsync);
            }

            return dataObjects;
        }

        #endregion

        // .. update by Id; get values from any instance
        private async Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(Guid id, T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            var results =
                await
                    UpdateWhere<T>(o => o.id == id, model => { model.UpdateFromAnotherObject(src, nameof(model.id)); }, overwriteReadOnly);

            return results.Single();
        }
    }
}