using System.Collections.Generic;

namespace DataStore
{
    using Interfaces;
    using Interfaces.Events;

    public class EventAggregator : IEventAggregator
    {
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        private EventAggregator(bool propogateDomainEvents)
        {
            PropogateDomainEvents = propogateDomainEvents;
        }

        public List<IEvent> Events { get; } = new List<IEvent>();

        public bool PropogateDataStoreEvents { get; } = true;

        public bool PropogateDomainEvents { get; }

        public IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Events.Add(@event);
            return new EventPropogator<TEvent>(
                @event,
                PropogateDomainEvents,
                PropogateDataStoreEvents,
                ReturnValues.ContainsKey(typeof(TEvent).FullName) ? ReturnValues[typeof(TEvent).FullName] : null);
        }

        public IValueReturner When<TEvent>() where TEvent : IEvent
        {
            return new ValueReturner(ReturnValues, typeof(TEvent).FullName);
        }

        public static IEventAggregator Create(bool propogateDomainEvents = true)
        {
            return new EventAggregator(propogateDomainEvents);
        }
    }
}