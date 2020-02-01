namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class EventReplay
    {
        private readonly IMessageAggregator messageAggregator;

        public EventReplay(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public List<T> ApplyAggregateEvents<T>(IEnumerable<T> results, Func<T, bool> predicate) where T : class, IAggregate, new()
        {
            //we are assuming that the predicate passed to us was also used to provide the list of results given to us
            var modifiedResults = results.ToList();
            var uncommittedEvents = this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>().OrderBy(e => e.Created).Where(e => !e.Committed);
            
            //events are replayed in the order they were added, update that would apply on an item added after the update will be ignored
            foreach (var eventAggregatorEvent in uncommittedEvents) ApplyEvent(modifiedResults, eventAggregatorEvent, predicate);

            return modifiedResults;
        }

        private static void ApplyEvent<T>(
            List<T> results,
            IQueuedDataStoreWriteOperation<T> previousUncommittedOperation,
            Func<T, bool> predicate)
            where T : class, IAggregate, new()
        {
            switch (previousUncommittedOperation)
            {
                case QueuedCreateOperation<T> _:

                    //failing this guard is not the result of replay, but it is the first time we can perform this test
                    Guard.Against(
                        results.Exists(result => result.id == previousUncommittedOperation.NewModel.id),
                        $"Item {previousUncommittedOperation.NewModel.id} has been queued to be created but it already exists.", 
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
                    //and it exists either from the db or an earlier part of the session
                    if (!predicate(previousUncommittedOperation.NewModel) && results.Exists(i => i.id == previousUncommittedOperation.NewModel.id))
                    {
                        results.Remove(results.Single(i => i.id == previousUncommittedOperation.NewModel.id));
                        break;
                    }

                    //if the requested resultset WOULD include this item after its been changed
                    //and it exists either from the db or an earlier part of the session
                    if (predicate(previousUncommittedOperation.NewModel) && results.Exists(i => i.id == previousUncommittedOperation.NewModel.id))
                    {
                        //update it
                        var itemToUpdate = results.Single(i => i.id == previousUncommittedOperation.NewModel.id);
                        previousUncommittedOperation.NewModel.CopyProperties(itemToUpdate);
                        break;
                    }

                    //if the requested resultset WOULD include this item after its been changed
                    //and it doesn't exist either from the db or an earlier part of the session
                    if (predicate(previousUncommittedOperation.NewModel) && !results.Exists(i => i.id == previousUncommittedOperation.NewModel.id))
                    {                        
                        //add it now in its updated state
                        //if we do this for a harddeleted item its ok because we will fail a guard that checks for updating previously harddeleted items
                        results.Add(previousUncommittedOperation.NewModel);
                        break;
                    }

                    break;
                case QueuedHardDeleteOperation<T> _:
                    //remove it altogether as it has been hard deleted and would not be returned under any circumstance unless you added it again later
                    //in the session
                    var itemToRemove2 = results.SingleOrDefault(i => i.id == previousUncommittedOperation.NewModel.id);
                    //there is no reason that the item you deleted earlier in the session will meet this queries predicate so we need to check for null
                    if (itemToRemove2 != null) results.Remove(itemToRemove2);               
                    break;
            }
        }
    }
}