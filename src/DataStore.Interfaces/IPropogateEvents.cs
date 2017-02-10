using System;
using System.Threading.Tasks;
using DataStore.Interfaces.Events;

namespace DataStore.Interfaces
{
    public interface IPropogateEvents<out T> 
        where T : IEvent
    {
        void ForwardTo(Action<T> passTo);

        TOut ForwardTo<TOut>(Func<T, TOut> passTo);

        Task ForwardToAsync(Func<T, Task> passTo);

        Task<TOut> ForwardToAsync<TOut>(Func<T, Task<TOut>> forwardTo);
    }
}