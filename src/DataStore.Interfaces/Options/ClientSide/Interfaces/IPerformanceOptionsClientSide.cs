namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IPerformanceOptionsClientSide<T> where T: IPerformanceOptionsClientSide<T>
    {
        T BypassRULimit(string reason);
    }
}