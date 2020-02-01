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
    using global::DataStore.Options;

    //Eventreplay on
    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly EventReplay eventReplay;

        private readonly IMessageAggregator messageAggregator;

        private readonly IncrementVersions incrementVersions;

        private readonly IDataStoreUpdateCapabilities updateCapabilities;

        public static void CheckWasObjectAlreadyHardDeleted<T>(IMessageAggregator messageAggregator, Guid aggregateId) where T : class, IAggregate, new()
        {
            var uncommittedEvents = messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(e => !e.Committed);

            Guard.Against(
                uncommittedEvents.ToList().Exists(e => e is QueuedHardDeleteOperation<T> && e.AggregateId == aggregateId),
                "Object has already been hard deleted earlier in the session you cannot do this",
                Guid.Parse("c53bef0f-a462-49cc-8d73-04cdbb3ea81c"));
        }

        private static void MarkAsSoftDeleted<T>(T dataObject) where T : class, IAggregate, new()
        {
            dataObject.Active = false;
        }

        public DataStoreDeleteCapabilities(
            IDocumentRepository dataStoreConnection,
            IDataStoreUpdateCapabilities updateCapabilities,
            IMessageAggregator messageAggregator,
            IncrementVersions incrementVersions)
        {
            this.updateCapabilities = updateCapabilities;
            this.messageAggregator = messageAggregator;
            this.incrementVersions = incrementVersions;
            DsConnection = dataStoreConnection;
            this.eventReplay = new EventReplay(messageAggregator);
        }

        private IDocumentRepository DsConnection { get; }

        public Task<T> DeleteHard<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            return DeleteHardById<T>(instance.id, methodName);
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            return (await DeleteHardWhere<T>(x => x.id == id, methodName).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var objectsToDelete = await this.messageAggregator
                                            .CollectAndForward(new AggregatesQueriedOperation<T>(methodName, DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                                            .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            //can't just return null here if there are no matches because we need to replay previous events
            //a match might have been added previously in this session            
            objectsToDelete = this.eventReplay.ApplyAggregateEvents(objectsToDelete, predicate.Compile());

            foreach (var dataObject in objectsToDelete)
                CheckWasObjectAlreadyHardDeleted<T>(this.messageAggregator, dataObject.id);

            if (!objectsToDelete.Any()) return objectsToDelete;

            var clones = new List<T>();

            foreach (var dataObject in objectsToDelete)
            {
                var clone = dataObject.Clone();

                void EtagUpdated(string newTag) => clone.Etag = newTag;

                this.messageAggregator.Collect(new QueuedHardDeleteOperation<T>(methodName, dataObject, DsConnection, this.messageAggregator, EtagUpdated));

                await this.incrementVersions.IncrementAggregateVersionOfQueuedItem(dataObject, methodName);
                await this.incrementVersions.DeleteAggregateHistory<T>(dataObject.id, methodName);

                clone.Etag = "waiting to be committed";
                clones.Add(clone);
            }

            //clone otherwise its to easy to change the referenced object before committing
            return clones;
        }

        public Task<T> DeleteSoft<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            return DeleteSoftById<T>(instance.id, methodName);
        }

        public Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            return this.updateCapabilities.UpdateById<T, UpdateOptions>(id, MarkAsSoftDeleted, o => o.DisableOptimisticConcurrecy(), true, methodName);
        }

        // .. soft delete one or more DataObjects 
        public Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            return this.updateCapabilities.UpdateWhere<T, UpdateOptions>(
                predicate,
                MarkAsSoftDeleted,
                o => o.DisableOptimisticConcurrecy(),
                true,
                nameof(DeleteSoftWhere));
        }

    }
}