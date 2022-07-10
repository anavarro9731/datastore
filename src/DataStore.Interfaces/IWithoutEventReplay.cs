namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;

    public interface IWithoutEventReplay
    {


        Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();
        
        Task<int> Count<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null) where T : class, IAggregate, new() where O : ClientSideReadOptions, new();

        Task<int> CountActive<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null) where T : class, IAggregate, new() where O : ClientSideReadOptions, new();



        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Action<ClientSideWithoutReplayOptions<T>> setOptions = null)
            where T : class, IAggregate, new();
        
        Task<IEnumerable<R>> Read<T, R>(Expression<Func<T, R>> map, Expression<Func<T, bool>> predicate = null, Action<ClientSideWithoutReplayOptions<R>> setOptions = null)
            where T : class, IAggregate, new() where R : class, IAggregate, new();
        
        Task<IEnumerable<R>> Read<T, O, R>(Expression<Func<T, R>> map, Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : ClientSideWithoutReplayOptions<R>, new() where R : class, IAggregate, new();

        
        Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null, Action<ClientSideWithoutReplayOptions<T>> setOptions = null)
            where T : class, IAggregate, new();
        
        Task<IEnumerable<R>> ReadActive<T, R>(Expression<Func<T, R>> map, Expression<Func<T, bool>> predicate = null, Action<ClientSideWithoutReplayOptions<R>> setOptions = null)
            where T : class, IAggregate, new() where R : class, IAggregate, new();
        
        Task<IEnumerable<R>> ReadActive<T, O, R>(Expression<Func<T, R>> map, Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : ClientSideWithoutReplayOptions<R>, new() where R : class, IAggregate, new();

        
        
        Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null)
            where T : class, IAggregate, new() where O : ClientSideWithoutReplayOptions<T>, new();

        Task<T> ReadById<T>(Guid modelId, Action<ClientSideWithoutReplayOptions<T>> setOptions = null) where T : class, IAggregate, new();
    }
}