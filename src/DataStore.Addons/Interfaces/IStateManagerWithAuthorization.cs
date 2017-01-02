namespace DataStore.Addons.Interfaces
{
    public interface IStateManagerWithAuthorization : IStateManager
    {
        ISecureDataStore DocumentDbPrimary { get; }
    }
}