namespace DataStore.DataAccess.Interfaces.Addons
{
    using DataAccess.Interfaces;

    //using Apogee.Storage;
    
    public interface IStateManagerWithoutAuthorization : IStateManager
    {
        IDataStore DocumentDbPrimary { get; }
      //  IFileStorageProvider FileStoragePrimary { get; }
    }
}