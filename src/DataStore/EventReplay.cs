using DataStore.Models.Messages;

namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    public class EventReplay
    {
        private readonly IMessageAggregator messageAggregator;

        public EventReplay(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public List<T> ApplyAggregateEvents<T>(IEnumerable<T> results, bool isReadActive) where T : class, IAggregate, new()
        {
            var modifiedResults = results.ToList();
            var uncommittedEvents =
                messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>().OrderBy(e => e.Created).Where(e => !e.Committed);

            foreach (var eventAggregatorEvent in uncommittedEvents)
                ApplyEvent(modifiedResults, eventAggregatorEvent, isReadActive);

            return modifiedResults;
        }

        private static void ApplyEvent<T>(List<T> results, IQueuedDataStoreWriteOperation<T> previousUncommittedOperation,
            bool requestingOnlyReadActive) where T : class, IAggregate, new()
        {
            if (previousUncommittedOperation is QueuedCreateOperation<T>)
            {
                if (requestingOnlyReadActive && !previousUncommittedOperation.Model.Active)
                {
                }
                else
                {
                    results.Add(previousUncommittedOperation.Model);
                }
            }
            else if (results.Exists(i => i.id == previousUncommittedOperation.Model.id))
            {
                if (previousUncommittedOperation is QueuedUpdateOperation<T>)
                    if (requestingOnlyReadActive && !previousUncommittedOperation.Model.Active)
                    {
                        var itemToRemove = results.Single(i => i.id == previousUncommittedOperation.Model.id);
                        results.Remove(itemToRemove);
                    }
                    else
                    {
                        var itemToUpdate = results.Single(i => i.id == previousUncommittedOperation.Model.id);
                        previousUncommittedOperation.Model.CopyProperties(itemToUpdate);
                    }

                if (previousUncommittedOperation is QueuedSoftDeleteOperation<T>)
                    if (requestingOnlyReadActive)
                    {
                        var itemToRemove = results.Single(i => i.id == previousUncommittedOperation.Model.id);
                        results.Remove(itemToRemove);
                    }

                if (previousUncommittedOperation is QueuedHardDeleteOperation<T>)
                {
                    var itemToRemove = results.Single(i => i.id == previousUncommittedOperation.Model.id);
                    results.Remove(itemToRemove);
                }
            }
        }
    }
}