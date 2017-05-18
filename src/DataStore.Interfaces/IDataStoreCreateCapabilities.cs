namespace DataStore.Interfaces
{
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreCreateCapabilities
    {
        Task<T> Create<T>(T model, bool readOnly = false) where T : class, IAggregate, new();
    }
}