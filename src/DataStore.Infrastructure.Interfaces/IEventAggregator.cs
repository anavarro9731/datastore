namespace Infrastructure.Interfaces
{
    using System.Collections.Generic;
    using Messages;

    public interface IEventAggregator
    {
        List<Event> Events { get; }

        bool PropogateDomainEvents { get; set; }

        IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : Event;

        IValueReturner When<TEvent>() where TEvent : Event;
    }
}