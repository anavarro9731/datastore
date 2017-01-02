namespace DataStore.Addons.Interfaces
{
    using global::DataStore.Interfaces;

    public interface IStateManagerWithoutAuthorization : IStateManager
    {
        IDataStore DocumentDbPrimary { get; }
    }
}