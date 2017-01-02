namespace DataStore
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using Interfaces.Addons;
    using Interfaces.Events;

    public class EventPropogator<TEvent> : IPropogateEvents<TEvent>
        where TEvent : IEvent
    {
        private readonly TEvent _event;

        private readonly bool _propogateDataStoreEvents;

        private readonly bool _propogateDomainEvents;

        private readonly object _toReturn;

        internal EventPropogator(TEvent @event, bool propogateDomainEvents, bool propogateDataStoreEvents, object toReturn)
        {
            this._event = @event;
            this._propogateDomainEvents = propogateDomainEvents;
            this._propogateDataStoreEvents = propogateDataStoreEvents;
            this._toReturn = toReturn;
        }
       
        public async Task<TOut> ForwardToAsync<TOut>(Func<TEvent, Task<TOut>> forwardTo)
        {
            if ((_propogateDomainEvents && !(_event is IDataStoreEvent)) || (_propogateDataStoreEvents && _event is IDataStoreEvent))
            {
                return await forwardTo(_event);
            }

            return _toReturn != null ? (TOut)_toReturn : default(TOut);
        }

        public Task ForwardToAsync(Func<TEvent, Task> forwardTo) 
        {
            if ((_propogateDomainEvents && !(_event is IDataStoreEvent)) || (_propogateDataStoreEvents && _event is IDataStoreEvent))
            {
                forwardTo(_event);
            }

            return Task.FromResult(false);
        }

        public void ForwardTo(Action<TEvent> passTo)
        {
            if ((_propogateDomainEvents && !(_event is IDataStoreEvent)) || (_propogateDataStoreEvents && _event is IDataStoreEvent))
            {
                passTo(_event);
            }
        }

        public TOut ForwardTo<TOut>(Func<TEvent, TOut> passTo)
        {
            if ((_propogateDomainEvents && !(_event is IDataStoreEvent)) || (_propogateDataStoreEvents && _event is IDataStoreEvent))
            {
                var returnValue = passTo(_event);

                return returnValue;
            }
            
            return _toReturn != null ? (TOut)_toReturn : default(TOut);
        }
    }
}