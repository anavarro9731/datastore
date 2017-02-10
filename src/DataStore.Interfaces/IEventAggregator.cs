namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using Events;

    public interface IEventAggregator
    {
        List<IEvent> Events { get; }

        bool PropogateDomainEvents { get; }

        bool PropogateDataStoreEvents { get; }

        IPropogateEvents<TEvent> Store<TEvent>(TEvent @event) where TEvent : IEvent;

        IValueReturner When<TEvent>() where TEvent : IEvent;
    }
}