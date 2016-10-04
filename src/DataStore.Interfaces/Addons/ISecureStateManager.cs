namespace DataStore.DataAccess.Interfaces.Addons
{
    public interface ISecureStateManager : IStateManager
    {
        ISecureDataStore GlobalStore { get; }
    }
}