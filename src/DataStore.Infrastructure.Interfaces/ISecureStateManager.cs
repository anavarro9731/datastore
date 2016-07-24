namespace Infrastructure.HandlerServiceInterfaces
{
    public interface ISecureStateManager : IStateManager
    {
        ISecureDataStore GlobalStore { get; }
    }
}