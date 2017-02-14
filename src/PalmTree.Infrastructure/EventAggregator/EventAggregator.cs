using System;
using System.Collections.Generic;
using PalmTree.Infrastructure.Interfaces;

namespace PalmTree.Infrastructure.EventAggregator
{
    public class EventAggregator : IEventAggregator
    {
        public IEnumerable<Type> WhiteListedForPropogation { get; }

        private EventAggregator(IEnumerable<Type> whitelistedForPropogation)
        {
            this.WhiteListedForPropogation = whitelistedForPropogation;
        }

        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public List<IEvent> Events { get; } = new List<IEvent>();


        public IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Events.Add(@event);
            return new EventPropogator<TEvent>(
                @event,
                this.WhiteListedForPropogation,
                ReturnValues.ContainsKey(typeof(TEvent).FullName) ? ReturnValues[typeof(TEvent).FullName] : null);
        }

        public IValueReturner When<TEvent>() where TEvent : IEvent
        {
            return new ValueReturner(ReturnValues, typeof(TEvent).FullName);
        }

        public static IEventAggregator Create(IEnumerable<Type> whiteListedForPropogation)
        {
            return new EventAggregator(whiteListedForPropogation);
        }

        public static IEventAggregator Create()
        {
            return new EventAggregator(null);
        }
    }
}