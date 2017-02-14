using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PalmTree.Infrastructure.Interfaces;
using PalmTree.Infrastructure.PureFunctions.Extensions;

namespace PalmTree.Infrastructure.EventAggregator
{
    public class EventPropogator<TEvent> : IPropogateEvents<TEvent>
        where TEvent : IEvent
    {
        private readonly TEvent _event;

        private readonly IEnumerable<Type> whiteListedForPropogation;

        private readonly object toReturn;

        internal EventPropogator(TEvent @event, IEnumerable<Type> whiteListedForPropogation, object toReturn)
        {
            this._event = @event;
            this.whiteListedForPropogation = whiteListedForPropogation;
            this.toReturn = toReturn;
        }
       
        public async Task<TOut> ForwardToAsync<TOut>(Func<TEvent, Task<TOut>> forwardTo)
        {
            if (this.whiteListedForPropogation == null || 
                this.whiteListedForPropogation.Any(x => _event.GetType().InheritsOrImplements(x)))
            {
                return await forwardTo(_event);
            }

            return toReturn != null ? (TOut)toReturn : default(TOut);
        }

        public Task ForwardToAsync(Func<TEvent, Task> forwardTo) 
        {
            if (this.whiteListedForPropogation == null ||
                this.whiteListedForPropogation.Any(x => _event.GetType().InheritsOrImplements(x)))
            {
                forwardTo(_event);
            }

            return Task.FromResult(false);
        }

        public void ForwardTo(Action<TEvent> passTo)
        {
            if (this.whiteListedForPropogation == null ||
                this.whiteListedForPropogation.Any(x => _event.GetType().InheritsOrImplements(x)))
            {
                passTo(_event);
            }
        }

        public TOut ForwardTo<TOut>(Func<TEvent, TOut> passTo)
        {
            if (this.whiteListedForPropogation == null ||
                this.whiteListedForPropogation.Any(x => _event.GetType().InheritsOrImplements(x)))
            {
                var returnValue = passTo(_event);

                return returnValue;
            }
            
            return toReturn != null ? (TOut)toReturn : default(TOut);
        }
    }
}