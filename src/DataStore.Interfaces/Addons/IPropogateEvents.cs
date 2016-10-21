using DataStore.DataAccess.Interfaces.Events;

namespace DataStore.DataAccess.Interfaces.Addons
{
    using System;
    using System.Threading.Tasks;

    public interface IPropogateEvents<out T> 
        where T : IEvent
    {
        void ForwardTo(Action<T> passTo);

        TOut ForwardTo<TOut>(Func<T, TOut> passTo);

        Task ForwardToAsync(Func<T, Task> passTo);

        Task<TOut> ForwardToAsync<TOut>(Func<T, Task<TOut>> forwardTo);
    }
}