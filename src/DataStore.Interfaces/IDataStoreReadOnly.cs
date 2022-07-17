namespace DataStore.Interfaces
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options.ClientSide;

    #endregion

    public interface IDataStoreReadOnly
    {
        IWithoutEventReplay WithoutEventReplay { get; }

        Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSideBase, new();

        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Action<ReadOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSideBase, new();

        Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null, Action<ReadOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> ReadActiveById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSideBase, new();

        Task<T> ReadActiveById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadActiveById<T>(string longId, Action<ReadOptionsClientSideBase> setOptions = null, string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSideBase, new();

        Task<T> ReadById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadById<T>(string longId, Action<ReadOptionsClientSideBase> setOptions = null, string methodName = null) where T : class, IAggregate, new();
    }
}