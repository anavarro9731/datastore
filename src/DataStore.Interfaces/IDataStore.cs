namespace DataStore.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStore : IDisposable, IDataStoreCreateCapabilities, IDataStoreQueryCapabilities, IDataStoreDeleteCapabilities, IDataStoreUpdateCapabilities

    {
        IWithoutEventReplay WithoutEventReplay { get; }

        IReadOnlyList<IDataStoreOperation> ExecutedOperations { get; }

        IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations { get; }

        IDataStoreQueryCapabilities AsReadOnly();

        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new();

        Task CommitChanges();
    }
}