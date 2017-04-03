namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using Models.Messages.Events;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    public class EventReplay
    {
        private readonly IMessageAggregator messageAggregator;

        public EventReplay(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public List<T> ApplyAggregateEvents<T>(IEnumerable<T> results, bool isReadActive) where T : IAggregate
        {
            var modifiedResults = results.ToList();
            var uncommittedEvents =
                messageAggregator.AllMessages.OfType<IDataStoreWriteEvent<T>>().OrderBy(e => e.Created).Where(e => !e.Committed);

            foreach (var eventAggregatorEvent in uncommittedEvents)
                ApplyEvent(modifiedResults, eventAggregatorEvent, isReadActive);

            return modifiedResults;
        }

        private static void ApplyEvent<T>(List<T> results, IDataStoreWriteEvent<T> eventAggregatorEvent,
            bool requestingOnlyReadActive) where T : IAggregate
        {
            if (eventAggregatorEvent is AggregateAdded<T>)
            {
                if (requestingOnlyReadActive && !eventAggregatorEvent.Model.Active)
                {
                }
                else
                {
                    results.Add(eventAggregatorEvent.Model);
                }
            }
            else if (results.Exists(i => i.id == eventAggregatorEvent.Model.id))
            {
                if (eventAggregatorEvent is AggregateUpdated<T>)
                    if (requestingOnlyReadActive && !eventAggregatorEvent.Model.Active)
                    {
                        var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                        results.Remove(itemToRemove);
                    }
                    else
                    {
                        var itemToUpdate = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                        eventAggregatorEvent.Model.CopyProperties(itemToUpdate);
                    }

                if (eventAggregatorEvent is AggregateSoftDeleted<T>)
                    if (requestingOnlyReadActive)
                    {
                        var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                        results.Remove(itemToRemove);
                    }

                if (eventAggregatorEvent is AggregateHardDeleted<T>)
                {
                    var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                    results.Remove(itemToRemove);
                }
            }
        }
    }
}