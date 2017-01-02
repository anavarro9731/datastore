namespace DataStore.Interfaces
{
    using System.Threading.Tasks;

    public interface IDataStoreCreateCapabilitiesScoped<T> where T : IAggregate, new()
    {
        Task<T> Create(T model, bool readOnly = false);
    }
}