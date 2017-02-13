using System.Collections.Generic;
using PalmTree.Infrastructure.Interfaces;

namespace PalmTree.Infrastructure.EventAggregator
{
    public class EventAggregator : IEventAggregator
    {
        private EventAggregator(bool propogateEvents)
        {
            this.PropogateEvents = propogateEvents;
        }

        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public List<IEvent> Events { get; } = new List<IEvent>();

        public bool PropogateEvents { get; } 

        public IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Events.Add(@event);
            return new EventPropogator<TEvent>(
                @event,
                PropogateEvents,
                ReturnValues.ContainsKey(typeof(TEvent).FullName) ? ReturnValues[typeof(TEvent).FullName] : null);
        }

        public IValueReturner When<TEvent>() where TEvent : IEvent
        {
            return new ValueReturner(ReturnValues, typeof(TEvent).FullName);
        }

        public static IEventAggregator Create(bool propogateEvents = true)
        {
            return new EventAggregator(propogateEvents);
        }
    }
}