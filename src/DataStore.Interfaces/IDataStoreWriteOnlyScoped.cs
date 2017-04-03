namespace DataStore.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreWriteOnlyScoped<T> :
        IDataStoreCreateCapabilitiesScoped<T>,
        IDataStoreDeleteCapabilitiesScoped<T>,
        IDataStoreUpdateCapabilitiesScoped<T>,
        IDisposable where T : IAggregate, new()

    {
        Task CommitChanges();
    }
}