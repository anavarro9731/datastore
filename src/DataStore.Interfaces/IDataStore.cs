namespace DataStore.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IDataStore : IDisposable, 
                                  IDataStoreCreateCapabilities, 
                                  IDataStoreQueryCapabilities, 
                                  IDataStoreDeleteCapabilities, 
                                  IDataStoreUpdateCapabilities
                                  
    {
        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : IAggregate, new();
        IDataStoreQueryCapabilities AsReadOnly();
        Task CommitChanges();
        IAdvancedCapabilities Advanced { get; }
    }
}