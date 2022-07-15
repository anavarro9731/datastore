namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    #endregion

    public class EventReplay
    {
        private readonly IMessageAggregator messageAggregator;

        private static void ApplyQueuedOperationToResultset<T>(
            List<T> results,
            IQueuedDataStoreWriteOperation<T> previousUncommittedOperation,
            Func<T, bool> predicate) where T : class, IAggregate, new()
        {
            switch (previousUncommittedOperation)
            {
                case QueuedCreateOperation<T> _:

                    //failing this guard is not the result of replay, but it is the first time we can perform this test
                    Guard.Against(
                        results.Exists(result => result.id == previousUncommittedOperation.AggregateId),
                        $"Item {previousUncommittedOperation.AggregateId} has been queued to be created but it already exists.",
                        Guid.Parse("bb0ddf49-ccce-4588-ae74-5724fcdb8638"));

                    if (predicate(previousUncommittedOperation.NewModel))
                    {
                        //if the requested resultset would normally include this item add it
                        results.Add(previousUncommittedOperation.NewModel);
                        /* items added which do not meet the current query's predicate
                         but which are updated by subsequent queued updates are handled 
                         in a special way by adding the item to the results based on the model in the update operation
                         therefore it does not need to be added here to be available to update later*/
                    }

                    break;
                case QueuedUpdateOperation<T> _:

                    //the requested resultset WOULD NOT include this item after it has been changed (including softdeletes)
                    //and it exists in the resultset now
                    if (!predicate(previousUncommittedOperation.NewModel)
                        && results.Exists(i => i.id == previousUncommittedOperation.AggregateId))
                    {
                        results.Remove(results.Single(i => i.id == previousUncommittedOperation.AggregateId));
                        break;
                    }

                    //if the requested resultset WOULD include this item after its been changed
                    //and it exists in the resultset now
                    if (predicate(previousUncommittedOperation.NewModel)
                        && results.Exists(i => i.id == previousUncommittedOperation.AggregateId))
                    {
                        //update it
                        var itemToUpdate = results.Single(i => i.id == previousUncommittedOperation.AggregateId);
                        previousUncommittedOperation.NewModel.CopyPropertiesTo(itemToUpdate);
                        break;
                    }

                    //if the requested resultset WOULD include this item after its been changed
                    //and it doesn't exist in the resultset now
                    if (predicate(previousUncommittedOperation.NewModel)
                        && !results.Exists(i => i.id == previousUncommittedOperation.AggregateId))
                    {
                        //add it now in its updated state
                        //if we do this for a harddeleted item its ok because we will fail a guard that checks for updating previously harddeleted items
                        results.Add(previousUncommittedOperation.NewModel);
                    }

                    break;
                case QueuedHardDeleteOperation<T> _:
                    //* remove it altogether as it has been hard deleted and would not be returned under any circumstances unless you added it again later in the session
                    var itemToRemove2 = results.SingleOrDefault(i => i.id == previousUncommittedOperation.AggregateId);
                    //* there is no reason to believe that the item you deleted earlier in the session will meet this queries predicate so we need to check for null
                    if (itemToRemove2 != null) results.Remove(itemToRemove2);
                    break;
            }
        }

        public EventReplay(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        /// <summary>
        ///     Important note: this function uses the items in the session to affect the results returned directly
        ///     it does NOT affect the items in the session itself as opposed to RemoveQueuedOperationsMatchingPredicate which
        ///     affects the items in the session themselves. This method also does not where I can recall it is used
        ///     take into account the operation that is being/added/removed/created from which it is called.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> ApplyQueuedOperations<T>(IEnumerable<T> results, Func<T, bool> predicate) where T : class, IAggregate, new()
        {
            //* we are assuming that the predicate passed to us was also used to provide the list of results given to us
            var modifiedResults = results.ToList();

            var uncommittedEvents = GetUncommitedEvents<T>();
            //* events are replayed in the order they were added
            foreach (var eventAggregatorEvent in uncommittedEvents)
                ApplyQueuedOperationToResultset(modifiedResults, eventAggregatorEvent, predicate);

            return modifiedResults;
        }

        public List<IQueuedDataStoreWriteOperation<T>> GetUncommitedEvents<T>() where T : class, IAggregate, new() =>
            this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>().OrderBy(e => e.Created).Where(e => !e.Committed)
                .ToList();

        public T MergeCurrentUpdateIntoPreviousCreateOrUpdateOperations<T>(
            Guid objectId,
            Action<T> updateAction,
            string methodCalled) where T : class, IAggregate, new()
        {
            //* we are assuming that the predicate passed to us was also used to provide the list of results given to us
            {
                var uncommittedEvents = GetUncommitedEvents<T>();

                foreach (var operation in uncommittedEvents)
                    if (operation.AggregateId == objectId && operation.GetType().InheritsOrImplements(typeof(ICanBeUpdatedWhileQueued<>)))
                    {
                        var updateable = operation as ICanBeUpdatedWhileQueued<T>;
                        updateable.UpdateModelWhileQueued(methodCalled, updateAction);
                        return operation.NewModel; /* should ever be only one previous match, 
                                I don't see how you could get more than one operation in the queue for the same object because of this merge code */
                    }
                return null;
            }
        }

        public void RemoveQueuedOperationsMatchingPredicate<T>(Func<T, bool> predicate, out List<T> itemsCreatedInThisSession)
            where T : class, IAggregate, new()
        {
            itemsCreatedInThisSession = new List<T>();
            var uncommittedEvents = GetUncommitedEvents<T>();

            foreach (var operation in uncommittedEvents)
            {
                switch (operation)
                {
                    case QueuedCreateOperation<T> _:
                        if (predicate(operation.NewModel))
                        {
                            itemsCreatedInThisSession.Add(operation.NewModel);
                            RemoveOperationFromQueue();
                        }
                        break;

                    case QueuedUpdateOperation<T> _:
                        if (predicate(operation.NewModel)) RemoveOperationFromQueue();

                        break;
                    case QueuedHardDeleteOperation<T> _:
                        if (predicate(operation.PreviousModel)) RemoveOperationFromQueue();

                        break;
                }

                void RemoveOperationFromQueue() => this.messageAggregator.Remove(operation);
            }
        }
    }
}