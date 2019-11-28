namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStore : IDisposable, IDataStoreCreateCapabilities, IDataStoreQueryCapabilities, IDataStoreDeleteCapabilities, IDataStoreUpdateCapabilities

    {
        IDocumentRepository DocumentRepository { get; }

        IReadOnlyList<IDataStoreOperation> ExecutedOperations { get; }

        IMessageAggregator MessageAggregator { get; }

        IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations { get; }

        IWithoutEventReplay WithoutEventReplay { get; }

        IDataStoreQueryCapabilities AsReadOnly();

        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new();

        Task CommitChanges();
    }
}