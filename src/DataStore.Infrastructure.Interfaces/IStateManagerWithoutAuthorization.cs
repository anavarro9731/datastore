namespace DataStore.Infrastructure.Interfaces
{
    using DataAccess.Interfaces;

    //using Apogee.Storage;
    
    public interface IStateManagerWithoutAuthorization : IStateManager
    {
        IDataStore DocumentDbPrimary { get; }
      //  IFileStorageProvider FileStoragePrimary { get; }
    }
}