namespace DataStore.Infrastructure.Impl
{
    using System.Collections.Generic;
    using Interfaces;
    using Messages;

    public class EventAggregator : IEventAggregator
    {
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public List<Event> Events { get; } = new List<Event>();

        public bool PropogateDataStoreEvents { get; set; } = true;

        public bool PropogateDomainEvents { get; set; } = true;

        public IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : Event
        {
            Events.Add(@event);
            return new EventPropogator<TEvent>(
                @event,
                PropogateDomainEvents,
                PropogateDataStoreEvents,
                ReturnValues.ContainsKey(typeof(TEvent).FullName) ? ReturnValues[typeof(TEvent).FullName] : null);
        }

        public IValueReturner When<TEvent>() where TEvent : Event
        {
            return new ValueReturner(this.ReturnValues, typeof(TEvent).FullName);
        }
    }
}