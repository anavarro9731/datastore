namespace Infrastructure.HandlerServices
{
    using System;
    using System.Threading.Tasks;

    using DataAccess.Messages.Events;

    using Infrastructure.HandlerServiceInterfaces;
    using Infrastructure.Messages;

    public class EventPropogator<TEvent> : IPropogateEvents<TEvent>
        where TEvent : Event
    {
        private readonly TEvent @event;

        private readonly bool propogateDataStoreEvents;

        private readonly bool propogateDomainEvents;

        private readonly object toReturn;

        internal EventPropogator(TEvent @event, bool propogateDomainEvents, bool propogateDataStoreEvents, object toReturn)
        {
            this.@event = @event;
            this.propogateDomainEvents = propogateDomainEvents;
            this.propogateDataStoreEvents = propogateDataStoreEvents;
            this.toReturn = toReturn;
        }

        public async Task<TOut> ForwardToAsync<TOut>(Func<TEvent, Task<TOut>> forwardTo)
        {
            if ((propogateDomainEvents && !(@event is IDataStoreEvent)) || (propogateDataStoreEvents && @event is IDataStoreEvent))
            {
                return await forwardTo(@event);
            }

            return toReturn != null ? (TOut)toReturn : default(TOut);
        }

        public Task ForwardToAsync(Func<TEvent, Task> forwardTo) 
        {
            if ((propogateDomainEvents && !(@event is IDataStoreEvent)) || (propogateDataStoreEvents && @event is IDataStoreEvent))
            {
                forwardTo(@event);
            }

            return Task.FromResult(false);
        }

        public void ForwardTo(Action<TEvent> passTo)
        {
            if ((propogateDomainEvents && !(@event is IDataStoreEvent)) || (propogateDataStoreEvents && @event is IDataStoreEvent))
            {
                passTo(@event);
            }
        }

        public TOut ForwardTo<TOut>(Func<TEvent, TOut> passTo)
        {
            if ((propogateDomainEvents && !(@event is IDataStoreEvent)) || (propogateDataStoreEvents && @event is IDataStoreEvent))
            {
                var returnValue = passTo(@event);

                return returnValue;
            }
            
            return toReturn != null ? (TOut)toReturn : default(TOut);
        }
    }
}