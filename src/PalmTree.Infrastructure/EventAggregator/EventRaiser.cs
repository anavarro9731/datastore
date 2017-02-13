using System;
using System.Threading.Tasks;
using PalmTree.Infrastructure.Interfaces;

namespace PalmTree.Infrastructure.EventAggregator
{
    public class EventPropogator<TEvent> : IPropogateEvents<TEvent>
        where TEvent : IEvent
    {
        private readonly TEvent _event;

        private readonly bool propogateEvents;

        private readonly object toReturn;

        internal EventPropogator(TEvent @event, bool propogateEvents, object toReturn)
        {
            this._event = @event;
            this.propogateEvents = propogateEvents;
            this.toReturn = toReturn;
        }
       
        public async Task<TOut> ForwardToAsync<TOut>(Func<TEvent, Task<TOut>> forwardTo)
        {
            if (propogateEvents)
            {
                return await forwardTo(_event);
            }

            return toReturn != null ? (TOut)toReturn : default(TOut);
        }

        public Task ForwardToAsync(Func<TEvent, Task> forwardTo) 
        {
            if (propogateEvents)
            {
                forwardTo(_event);
            }

            return Task.FromResult(false);
        }

        public void ForwardTo(Action<TEvent> passTo)
        {
            if (propogateEvents)
            {
                passTo(_event);
            }
        }

        public TOut ForwardTo<TOut>(Func<TEvent, TOut> passTo)
        {
            if (propogateEvents)
            {
                var returnValue = passTo(_event);

                return returnValue;
            }
            
            return toReturn != null ? (TOut)toReturn : default(TOut);
        }
    }
}