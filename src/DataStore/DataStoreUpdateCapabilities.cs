namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    //methods return object after changes have been applied, including previous uncommitted session changes

    internal class DataStoreUpdateCapabilities : IDataStoreUpdateCapabilities
    {
        private readonly IMessageAggregator eventAggregator;

        private readonly EventReplay eventReplay;

        public DataStoreUpdateCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventReplay = new EventReplay(eventAggregator);
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        // .. update using id; get values from another instance of the same aggregate
        public Task<T> Update<T>(T src, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            //clone, we don't want changes made at any point after this call, to affect the commit or the resulting events
            var cloned = src.Clone();

            //exclude these for the scenario where you try to update an object which
            //has been added in this session but has not yet been committed
            //because timestamps are set AFTER you pass the object to the datastore
            //if you passed to this function the original object you passed to the Create<T>() Function
            //it will attempt to overwrite the Created variables with NULL values from that instance
            var excludedParameters = new[]
            {
                nameof(IAggregate.Created),
                nameof(IAggregate.CreatedAsMillisecondsEpochTime),
                nameof(IAggregate.Modified),
                nameof(IAggregate.ModifiedAsMillisecondsEpochTime),

            };

            return UpdateByIdInternal<T>(src.id, model => cloned.CopyProperties(model, excludedParameters), overwriteReadOnly, methodName);
        }

        public Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            return UpdateByIdInternal(id, action, overwriteReadOnly, methodName);
        }

        // update a DataObject selected with a singular predicate
        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false,
            string methodName = null) where T : class, IAggregate, new()
        {
            var objectsToUpdate =
                await this.eventAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                          .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            var dataObjects = this.eventReplay.ApplyAggregateEvents(objectsToUpdate, predicate.Compile()).AsEnumerable();

            return UpdateInternal(action, dataObjects, overwriteReadOnly, methodName);
        }

        private async Task<T> UpdateByIdInternal<T>(Guid id, Action<T> action, bool overwriteReadOnly, string methodName) where T : class, IAggregate, new()
        {
            var objectToUpdate = await this.eventAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, id, typeof(T)))
                                           .To(DsConnection.GetItemAsync<T>).ConfigureAwait(false);

            //can't just return null here if the object doesn't exist because we need to replay previous events
            //the object might have been added previously in this session
            var list = new List<T>().Op(
                l =>
                    {
                        if (objectToUpdate != null) l.Add(objectToUpdate);
                    });
            var dataObjects = this.eventReplay.ApplyAggregateEvents(list, a => a.id == id);

            return UpdateInternal(action, dataObjects, overwriteReadOnly, methodName).SingleOrDefault();
        }

        private IEnumerable<T> UpdateInternal<T>(Action<T> action, IEnumerable<T> dataObjects, bool overwriteReadOnly, string methodName)
            where T : class, IAggregate, new()
        {
            foreach (var dataObject in dataObjects)
            {
                Guard.Against(dataObject.ReadOnly && !overwriteReadOnly, "Cannot update read-only item " + dataObject.id);
                DataStoreDeleteCapabilities.CheckWasObjectAlreadyHardDeleted<T>(this.eventAggregator, dataObject.id);
            }

            foreach (var dataObject in dataObjects)
            {
                var originalId = dataObject.id;

                var restrictedPropertiesBefore = originalId + dataObject.schema;
                var restrictedCreatedBefore = dataObject.Created.ToString(CultureInfo.InvariantCulture) + dataObject.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedBefore = dataObject.Modified.ToString(CultureInfo.InvariantCulture) + dataObject.ModifiedAsMillisecondsEpochTime;
                restrictedPropertiesBefore = restrictedPropertiesBefore + restrictedCreatedBefore + restrictedModifiedBefore;

                action(dataObject);

                var restrictedPropertiesAfter = originalId + dataObject.schema;
                var restrictedCreatedAfter = dataObject.Created.ToString(CultureInfo.InvariantCulture) + dataObject.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedAfter = dataObject.Modified.ToString(CultureInfo.InvariantCulture) + dataObject.ModifiedAsMillisecondsEpochTime;
                restrictedPropertiesAfter = restrictedPropertiesAfter + restrictedCreatedAfter + restrictedModifiedAfter;


                Guard.Against(
                    restrictedPropertiesBefore != restrictedPropertiesAfter,
                    "Cannot change restricted properties [id, schema, Created, CreatedAsMillisecondsEpochTime, Modified, ModifiedAsMillisecondsEpochTime on Aggregate "
                    + originalId);

                //don't allow this to be set to null by client
                dataObject.ScopeReferences = dataObject.ScopeReferences ?? new List<IScopeReference>();

                dataObject.Modified = DateTime.UtcNow;
                dataObject.ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToSecondsEpochTime();

                this.eventAggregator.Collect(new QueuedUpdateOperation<T>(methodName, dataObject, DsConnection, this.eventAggregator));
            }

            //clone otherwise its to easy to change the referenced object before committing
            return dataObjects.Select(d => d.Clone());
        }
    }
}