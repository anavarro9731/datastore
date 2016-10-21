namespace DataStore.DataAccess.Interfaces.Addons
{
    using DataAccess.Interfaces;

    public interface IStateManagerWithoutAuthorization : IStateManager
    {
        IDataStore DocumentDbPrimary { get; }
    
        //  IFileStorageProvider FileStoragePrimary { get; }

    }
}