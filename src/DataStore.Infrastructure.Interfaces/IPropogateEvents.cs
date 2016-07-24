namespace Infrastructure.HandlerServiceInterfaces
{
    using System;
    using System.Threading.Tasks;

    using Infrastructure.Messages;

    public interface IPropogateEvents<out T> 
        where T : Event
    {
        void ForwardTo(Action<T> passTo);

        TOut ForwardTo<TOut>(Func<T, TOut> passTo);

        Task ForwardToAsync(Func<T, Task> passTo);

        Task<TOut> ForwardToAsync<TOut>(Func<T, Task<TOut>> forwardTo);
    }
}