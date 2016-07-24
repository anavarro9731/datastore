namespace DataAccess.Interfaces
{
    using System;

    public interface IDataStore : IDisposable, 
                                  IDataStoreCreateCapabilities, 
                                  IDataStoreQueryCapabilities, 
                                  IDataStoreDeleteCapabilities, 
                                  IDataStoreUpdateCapabilities
    {
    }
}