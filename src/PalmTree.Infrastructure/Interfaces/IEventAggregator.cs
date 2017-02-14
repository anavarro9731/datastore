using System;
using System.Collections.Generic;
using PalmTree.Infrastructure.EventAggregator;

namespace PalmTree.Infrastructure.Interfaces
{
    public interface IEventAggregator
    {
        List<IEvent> Events { get; }

        IEnumerable<Type> WhiteListedForPropogation { get; }

        IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent;

        IValueReturner When<TEvent>() where TEvent : IEvent;
    }
}