namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.LowLevel;
    using Models.Messages.Events;
    using Models.PureFunctions;
    using Models.PureFunctions.Extensions;
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

        // .. update by id; get values from any instance
        private async Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(Guid id, T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            var results =
                await UpdateWhere<T>(o => o.id == id, model => src.CopyProperties(model), overwriteReadOnly);

            return results.Single();
        }

        #region IDataStoreUpdateCapabilities Members

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : IAggregate
        {
            var results = await UpdateWhere(o => o.id == id, action, overwriteReadOnly);

            return results.Single();
        }

        // .. update using id; get values from another instance
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
            var objects = await eventAggregator.CollectAndForward(new AggregatesQueried<T>(nameof(UpdateWhere),
                    DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .To(DsConnection.ExecuteQuery);

            objects = eventReplay.ApplyAggregateEvents(objects, false);

            var dataObjects = objects.AsEnumerable();

            Guard.Against(dataObjects.Any(dataObject => dataObject.ReadOnly && !overwriteReadOnly),
                "Cannot update read-only items");

            foreach (var dataObject in dataObjects)
            {
                var originalId = dataObject.id;
                var restrictedProperties = originalId +
                                           dataObject.schema +
                                           dataObject.Created +
                                           dataObject.CreatedAsMillisecondsEpochTime;

                action(dataObject);

                var restrictedProperties2 = dataObject.id +
                                            dataObject.schema +
                                            dataObject.Created +
                                            dataObject.CreatedAsMillisecondsEpochTime;

                //don't allow this to be set to null
                dataObject.ScopeReferences = dataObject.ScopeReferences ?? new List<IScopeReference>();

                Guard.Against(restrictedProperties2 != restrictedProperties,
                    "Cannot change restricted properties [id, schema, Created, CreatedAsMillisecondsEpochTime on entity " +
                    originalId);

                eventAggregator.Collect(new AggregateUpdated<T>(nameof(UpdateWhere), dataObject, DsConnection));
            }

            return dataObjects;
        }

        #endregion
    }
}