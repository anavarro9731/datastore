namespace DataStore.Interfaces
{
    #region

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;

    #endregion

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