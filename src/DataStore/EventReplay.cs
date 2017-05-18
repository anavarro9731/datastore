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

        private static void ApplyEvent<T>(List<T> results, IQueuedDataStoreWriteOperation<T> operationAggregatorOperation,
            bool requestingOnlyReadActive) where T : class, IAggregate, new()
        {
            if (operationAggregatorOperation is QueuedCreateOperation<T>)
            {
                if (requestingOnlyReadActive && !operationAggregatorOperation.Model.Active)
                {
                }
                else
                {
                    results.Add(operationAggregatorOperation.Model);
                }
            }
            else if (results.Exists(i => i.id == operationAggregatorOperation.Model.id))
            {
                if (operationAggregatorOperation is QueuedUpdateOperation<T>)
                    if (requestingOnlyReadActive && !operationAggregatorOperation.Model.Active)
                    {
                        var itemToRemove = results.Single(i => i.id == operationAggregatorOperation.Model.id);
                        results.Remove(itemToRemove);
                    }
                    else
                    {
                        var itemToUpdate = results.Single(i => i.id == operationAggregatorOperation.Model.id);
                        operationAggregatorOperation.Model.CopyProperties(itemToUpdate);
                    }

                if (operationAggregatorOperation is QueuedSoftDeleteOperation<T>)
                    if (requestingOnlyReadActive)
                    {
                        var itemToRemove = results.Single(i => i.id == operationAggregatorOperation.Model.id);
                        results.Remove(itemToRemove);
                    }

                if (operationAggregatorOperation is QueuedHardDeleteOperation<T>)
                {
                    var itemToRemove = results.Single(i => i.id == operationAggregatorOperation.Model.id);
                    results.Remove(itemToRemove);
                }
            }
        }
    }
}