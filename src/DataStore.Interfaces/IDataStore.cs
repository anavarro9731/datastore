using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStore.Interfaces.Events;

namespace DataStore.Interfaces
{
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