namespace Infrastructure.Interfaces
{
    //using Apogee.Storage;
    using DataAccess.Interfaces;

    public interface IStateManagerWithoutAuthorization : IStateManager
    {
        IDataStore DocumentDbPrimary { get; }
      //  IFileStorageProvider FileStoragePrimary { get; }
    }
}