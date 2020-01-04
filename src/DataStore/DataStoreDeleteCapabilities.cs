namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    //Eventreplay on
    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IDataStoreUpdateCapabilities updateCapabilities;

        private readonly IMessageAggregator messageAggregator;

        private readonly EventReplay eventReplay;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IDataStoreUpdateCapabilities updateCapabilities, IMessageAggregator messageAggregator)
        {
            this.updateCapabilities = updateCapabilities;
            this.messageAggregator = messageAggregator;
            DsConnection = dataStoreConnection;
            this.eventReplay = new EventReplay(messageAggregator);
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            return (await DeleteHardWhere<T>(x => x.id == id, methodName).ConfigureAwait(false)).SingleOrDefault();
        }

        public Task<T> DeleteHard<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            return DeleteHardById<T>(instance.id, methodName);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {

            var objectsToDelete = await this.messageAggregator
                                             .CollectAndForward(
                                                 new AggregatesQueriedOperation<T>(methodName, DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                                             .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            //can't just return null here if there are no matches because we need to replay previous events
            //a match might have been added previously in this session            
            objectsToDelete = this.eventReplay.ApplyAggregateEvents(objectsToDelete, predicate.Compile());

            foreach (var dataObject in objectsToDelete)
                CheckWasObjectAlreadyHardDeleted<T>(this.messageAggregator, dataObject.id);

            if (!objectsToDelete.Any()) return objectsToDelete;

            foreach (var dataObject in objectsToDelete)
                HardDeleteObject(methodName, dataObject);

            //clone otherwise its to easy to change the referenced object before committing
            return objectsToDelete.Select(d => d.Clone());
        }

        private void HardDeleteObject<T>(string methodName, T dataObject) where T : class, IAggregate, new()
        {            
            this.messageAggregator.Collect(new QueuedHardDeleteOperation<T>(methodName, dataObject, DsConnection, this.messageAggregator));
        }

        public static void CheckWasObjectAlreadyHardDeleted<T>(IMessageAggregator messageAggregator, Guid aggregateId) where T: class, IAggregate, new()
        {            
            var uncommittedEvents = messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(e => !e.Committed);

            Guard.Against(uncommittedEvents.ToList().Exists(e => e is QueuedHardDeleteOperation<T> && e.AggregateId == aggregateId),
                "Object has already been hard deleted earlier in the session you cannot do this", Guid.Parse("c53bef0f-a462-49cc-8d73-04cdbb3ea81c"));
        }

        public Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            return this.updateCapabilities.UpdateById<T>(id, MarkAsSoftDeleted, true, methodName);
        }

        public Task<T> DeleteSoft<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            return DeleteSoftById<T>(instance.id, methodName);
        }

        // .. soft delete one or more DataObjects 
        public Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            return this.updateCapabilities.UpdateWhere(predicate, MarkAsSoftDeleted, true, nameof(DeleteSoftWhere));
        }

        private static void MarkAsSoftDeleted<T>(T dataObject) where T : class, IAggregate, new()
        {
            dataObject.Active = false;
        }

    }
}