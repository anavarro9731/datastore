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
        IReadOnlyList<IDataStoreEvent> Events { get; }
        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : IAggregate, new();
        IDataStoreQueryCapabilities AsReadOnly();
        Task CommitChanges();
    }
}