namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IPerformanceOptionsClientSide
    {
        void BypassRULimit(string reason);
    }
}