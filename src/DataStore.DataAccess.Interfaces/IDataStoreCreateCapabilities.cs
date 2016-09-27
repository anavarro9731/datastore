namespace DataStore.DataAccess.Interfaces
{
    using System.Threading.Tasks;

    public interface IDataStoreCreateCapabilities
    {
        Task<T> Create<T>(T model, bool readOnly = false) where T : IAggregate, new();
    }
}