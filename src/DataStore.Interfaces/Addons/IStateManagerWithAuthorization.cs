namespace DataStore.DataAccess.Interfaces.Addons
{
    public interface IStateManagerWithAuthorization : IStateManager
    {
        ISecureDataStore DocumentDbPrimary { get; }
    }
}