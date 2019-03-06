namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IWithoutEventReplay
    {
        Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T>() where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T, O>(Action<O> setOptions) where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new();

        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new();

        Task<IEnumerable<T>> ReadActive<T>() where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T, O>(Action<O> setOptions) where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new();

        Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : class, IWithoutReplayOptions, new();

        Task<T> ReadById<T>(Guid modelId) where T : class, IAggregate, new();
    }
}