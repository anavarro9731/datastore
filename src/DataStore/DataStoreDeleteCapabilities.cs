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
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    //Eventreplay on
    internal class DataStoreDeleteCapabilities
    {
        private readonly EventReplay eventReplay;

        private readonly IncrementVersions incrementVersions;

        private readonly IMessageAggregator messageAggregator;

        private readonly DataStoreUpdateCapabilities updateCapabilities;

        public static void CheckWasObjectAlreadyHardDeleted<T>(IMessageAggregator messageAggregator, Guid aggregateId)
            where T : class, IAggregate, new()
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
            DataStoreUpdateCapabilities updateCapabilities,
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

        public Task<T> Delete<T, O>(T instance, O options, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsLibrarySide, new() =>
            DeleteById<T, O>(instance.id, options, methodName);

        public async Task<T> DeleteById<T, O>(Guid id, O options, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsLibrarySide, new()
        {
            return (await DeleteWhere<T, O>(x => x.id == id, options, methodName).ConfigureAwait(false)).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> DeleteWhere<T, O>(Expression<Func<T, bool>> predicate, O options, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsLibrarySide, new()
        {
            {
                if (options.IsHardDelete)
                {
                    List<T> objectsToDelete = null;

                    this.eventReplay.RemoveQueuedOperationsMatchingPredicate(predicate.Compile(), out var itemsCreatedInThisSession);

                    await GetObjectFromDatabaseThatMatchPredicateForDeletion(predicate, v => objectsToDelete = v).ConfigureAwait(false);

                    if (!objectsToDelete.Any()) return itemsCreatedInThisSession;

                    var results = new List<T>(itemsCreatedInThisSession); //* clone otherwise its too easy to change the referenced object before committing

                    foreach (var modelToPersist in objectsToDelete)
                    {
 
                        this.messageAggregator.Collect(
                            new QueuedHardDeleteOperation<T>(
                                methodName,
                                modelToPersist,
                                DsConnection,
                                this.messageAggregator
                                ));

                        await this.incrementVersions.IncrementAggregateVersionOfItemToBeQueued(modelToPersist, methodName).ConfigureAwait(false); 
                        await this.incrementVersions.DeleteAggregateHistory<T>(modelToPersist.id, methodName).ConfigureAwait(false);

                        var clone = modelToPersist.Clone();
                        clone.Etag = "waiting to be committed";
                        (modelToPersist as IEtagUpdated).EtagUpdated += s => clone.Etag = s;
                        results.Add(clone);
                    }

                    return results;
                }

                async Task GetObjectFromDatabaseThatMatchPredicateForDeletion(
                    Expression<Func<T, bool>> predicate1,
                    Action<List<T>> setObjectsToBeDeleted) 
                {
                    var objectToBeDeletedFromDb = await this.messageAggregator
                                                            .CollectAndForward(
                                                                new AggregatesQueriedOperation<T>(
                                                                    methodName,
                                                                    DsConnection.CreateQueryable<T>().Where(predicate1)))
                                                            .To(DsConnection.ExecuteQuery).ConfigureAwait(false);
                    setObjectsToBeDeleted(objectToBeDeletedFromDb.ToList());
                }
            }

            return await this.updateCapabilities.UpdateWhere(
                       predicate,
                       MarkAsSoftDeleted,
                       (UpdateOptionsLibrarySide)new DefaultUpdateOptions().Op(
                           o =>
                               {
                               o.DisableOptimisticConcurrency();
                               o.OverwriteReadonly();
                               }),
                       methodName).ConfigureAwait(false);
        }
    }
}
