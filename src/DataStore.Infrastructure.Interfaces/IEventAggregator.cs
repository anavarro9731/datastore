namespace Infrastructure.HandlerServiceInterfaces
{
    using System.Collections.Generic;

    using Infrastructure.Messages;

    public interface IEventAggregator
    {
        List<Event> Events { get; }

        bool PropogateDomainEvents { get; set; }

        IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : Event;

        IValueReturner When<TEvent>() where TEvent : Event;
    }
}