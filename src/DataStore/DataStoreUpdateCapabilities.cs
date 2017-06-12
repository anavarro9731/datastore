namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.LowLevel;
    using Models.Messages;
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

        #region

        public Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true)
            where T : class, IAggregate, new()
        {
            return UpdateByIdInternal(id, action, overwriteReadOnly);
        }

        // .. update using id; get values from another instance
        public Task<T> Update<T>(T src, bool overwriteReadOnly = true)
            where T : class, IAggregate, new()
        {            
            //clone, we don't want changes made at any point after this call, to affect the commit or the resulting events
            var cloned = src.Clone();
            return UpdateByIdInternal<T>(src.id, model => cloned.CopyProperties(model), overwriteReadOnly);
        }

        // update a DataObject selected with a singular predicate
        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : class, IAggregate, new()
        {
            var objectsToUpdate = await eventAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(nameof(UpdateWhere),
                    DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            return UpdateInternal(action, overwriteReadOnly, objectsToUpdate);
        }

        #endregion

        private async Task<T> UpdateByIdInternal<T>(Guid id, Action<T> action, bool overwriteReadOnly)
            where T : class, IAggregate, new()
        {
            var objectToUpdate = await eventAggregator
                .CollectAndForward(new AggregateQueriedByIdOperation(nameof(UpdateById), id, typeof(T)))
                .To(DsConnection.GetItemAsync<T>).ConfigureAwait(false);

            var list = new List<T>().Op(l =>
            {
                if (objectToUpdate != null) l.Add(objectToUpdate);
            });

            //can't just return null here because we need to reply previous events the object might have been added previously

            return UpdateInternal(action, overwriteReadOnly, list).SingleOrDefault();
        }

        private IEnumerable<T> UpdateInternal<T>(Action<T> action, bool overwriteReadOnly, IEnumerable<T> objects)
            where T : class, IAggregate, new()
        {
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

                eventAggregator.Collect(
                    new QueuedUpdateOperation<T>(nameof(UpdateWhere), dataObject, DsConnection, eventAggregator));
            }

            //clone otherwise its to easy to change the referenced object before committing
            return dataObjects.Select(d => d.Clone());
        }
    }
}