using DataStore.DataAccess.Interfaces.Events;

namespace DataStore.DataAccess.Interfaces.Addons
{
    using System.Collections.Generic;

    public interface IEventAggregator
    {
        List<IEvent> Events { get; }

        bool PropogateDomainEvents { get; set; }

        IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent;

        IValueReturner When<TEvent>() where TEvent : IEvent;
    }
}