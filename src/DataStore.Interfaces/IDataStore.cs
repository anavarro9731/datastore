namespace DataStore.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Events;
    using LowLevel;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    public interface IDataStore : IDisposable,
        IDataStoreCreateCapabilities,
        IDataStoreQueryCapabilities,
        IDataStoreDeleteCapabilities,
        IDataStoreUpdateCapabilities

    {
        IAdvancedCapabilities Advanced { get; }
        IReadOnlyList<IDataStoreOperation> ExecutedOperations { get; }
        IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations { get; }
        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new();
        IDataStoreQueryCapabilities AsReadOnly();
        Task CommitChanges();
    }
}