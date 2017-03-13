
namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Messages.Events;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    //methods return object after changes have been applied, including previous uncommitted session changes

    internal class DataStoreUpdateCapabilities : IDataStoreUpdateCapabilities
    {
        private readonly IMessageAggregator eventAggregator;
        private readonly EventReplay eventReplay;

        public DataStoreUpdateCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventReplay = new EventReplay(eventAggregator);
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        // .. update by Id; get values from any instance
        private async Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(Guid id, T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            var results =
                await
                    UpdateWhere<T>(o => o.id == id, model => { model.UpdateFromAnotherObject(src, nameof(model.id)); }, overwriteReadOnly);

            return results.Single();
        }

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
            var objects = await eventAggregator.CollectAndForward(new AggregatesQueried<T>(nameof(UpdateWhere), DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .To(DsConnection.ExecuteQuery);

            objects = eventReplay.ApplyAggregateEvents(objects, false);

            var dataObjects = objects.AsEnumerable();

            if (dataObjects.Any(dataObject => dataObject.ReadOnly && !overwriteReadOnly))
                throw new ApplicationException("Cannot update read-only items");

            foreach (var dataObject in dataObjects)
            {
                action(dataObject);
                eventAggregator.Collect(new AggregateUpdated<T>(nameof(UpdateWhere), dataObject, DsConnection));
            }

            return dataObjects;
        }

        #endregion
    }
}