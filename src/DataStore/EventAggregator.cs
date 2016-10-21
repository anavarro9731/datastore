namespace DataStore
{
    using System.Collections.Generic;

    using DataAccess.Interfaces.Addons;
    using DataAccess.Interfaces.Events;

    public class EventAggregator : IEventAggregator
    {
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public List<IEvent> Events { get; } = new List<IEvent>();

        public bool PropogateDataStoreEvents { get; set; } = true;

        public bool PropogateDomainEvents { get; set; } = true;

        public IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent
        {
            this.Events.Add(@event);
            return new EventPropogator<TEvent>(
                       @event,
                       this.PropogateDomainEvents,
                       this.PropogateDataStoreEvents,
                       this.ReturnValues.ContainsKey(typeof(TEvent).FullName) ? this.ReturnValues[typeof(TEvent).FullName] : null);
        }

        public IValueReturner When<TEvent>() where TEvent : IEvent
        {
            return new ValueReturner(this.ReturnValues, typeof(TEvent).FullName);
        }
    }
}