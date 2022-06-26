namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;

    public interface IDataStore : IDataStoreReadOnly, IDataStoreWriteOnly
    {

        IDataStoreOptions DataStoreOptions { get; }

        IDocumentRepository DocumentRepository { get; }

        IReadOnlyList<IDataStoreOperation> ExecutedOperations { get; }

        IMessageAggregator MessageAggregator { get; }

        IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations { get; }

        IDataStoreReadOnly AsReadOnly();

        IDataStoreWriteOnly AsWriteOnlyScoped<T>() where T : class, IAggregate, new();

        Task CommitChanges();
    }
}