namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using DataAccess.Interfaces.Events;
    using DataAccess.Models.Messages.Events;
    using Infrastructure.PureFunctions.PureFunctions.Extensions;

    public class EventReplay
    {
        private readonly IEventAggregator _eventAggregator;

        public EventReplay(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<T> ApplyAggregateEvents<T>(IEnumerable<T> results, bool isReadActive) where T : IAggregate
        {
            var modifiedResults = results.ToList();
            var uncommittedEvents = _eventAggregator.Events.OrderBy(e => e.OccurredAt).OfType<IDataStoreWriteEvent<T>>().Where(e => !e.Committed);

            foreach (var eventAggregatorEvent in uncommittedEvents)
            {
                ApplyEvent(modifiedResults, eventAggregatorEvent, isReadActive);
            }

            return modifiedResults;
        }

        private static void ApplyEvent<T>(IList<T> results, IDataStoreWriteEvent<T> eventAggregatorEvent, bool requestingOnlyReadActive) where T : IAggregate
        {
            if (eventAggregatorEvent is AggregateAdded<T>)
            {
                if (requestingOnlyReadActive && !eventAggregatorEvent.Model.Active) { }
                else
                {
                    results.Add(eventAggregatorEvent.Model);
                }
            }

            if (eventAggregatorEvent is AggregateUpdated<T>)
            {
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
            }

            if (eventAggregatorEvent is AggregateSoftDeleted<T>)
            {
                if (requestingOnlyReadActive)
                {
                    var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                    results.Remove(itemToRemove);
                }
            }

            if (eventAggregatorEvent is AggregateHardDeleted<T>)
            {
                var itemToRemove = results.Single(i => i.id == eventAggregatorEvent.Model.id);
                results.Remove(itemToRemove);
            }
        }
    }
}