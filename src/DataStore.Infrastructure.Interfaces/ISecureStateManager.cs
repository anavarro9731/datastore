namespace DataStore.Infrastructure.Interfaces
{
    public interface ISecureStateManager : IStateManager
    {
        ISecureDataStore GlobalStore { get; }
    }
}