using System.Threading.Tasks;

namespace DataStore.DataAccess.Interfaces
{
    using System;

    public interface IDataStore : IDisposable, 
                                  IDataStoreCreateCapabilities, 
                                  IDataStoreQueryCapabilities, 
                                  IDataStoreDeleteCapabilities, 
                                  IDataStoreUpdateCapabilities
    {
        IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : IAggregate, new();
        IDataStoreQueryCapabilities AsReadOnly();
        Task CommitChanges();
    }
}