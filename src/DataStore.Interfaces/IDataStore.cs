namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;

    public interface IDataStore : IDataStoreReadOnly, IDataStoreWriteOnly
    {
        IDataStoreOptions DataStoreOptions { get; }

        IDocumentRepository DocumentRepository { get; }

        IReadOnlyList<IDataStoreOperation> ExecutedOperations { get; }

        IMessageAggregator MessageAggregator { get; }

        IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations { get; }

        IWithoutEventReplay WithoutEventReplay { get; }

        IDataStoreReadOnly AsReadOnly();

        IDataStoreWriteOnly AsWriteOnlyScoped<T>() where T : class, IAggregate, new();

        Task CommitChanges();

        Task<T> Create<T, O>(T model, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : CreateOptionsClientSide, new();

        Task<T> Create<T>(T model, Action<CreateOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> Delete<T, O>(T instance, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new();

        Task<T> Delete<T>(T instance, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> DeleteById<T, O>(Guid id, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new();

        Task<T> DeleteById<T>(Guid id, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<IEnumerable<T>> DeleteWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new();

        Task<IEnumerable<T>> DeleteWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<DeleteOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T, O>(
            Expression<Func<T, bool>> predicate = null,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : ReadOptionsClientSide, new();

        Task<IEnumerable<T>> Read<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<ReadOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ReadActive<T, O>(
            Expression<Func<T, bool>> predicate = null,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : ReadOptionsClientSide, new();

        Task<IEnumerable<T>> ReadActive<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<ReadOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new();

        Task<T> ReadActiveById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new();

        Task<T> ReadActiveById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new();

        Task<T> ReadById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> Update<T, O>(T src, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new();

        Task<T> Update<T>(T src, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<T> UpdateById<T, O>(Guid id, Action<T> action, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new();

        Task<T> UpdateById<T>(Guid id, Action<T> action, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new();

        Task<IEnumerable<T>> UpdateWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new();

        Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<UpdateOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new();
    }
}