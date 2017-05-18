namespace DataStore.Interfaces
{
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreCreateCapabilitiesScoped<T> where T : class, IAggregate, new()
    {
        Task<T> Create(T model, bool readOnly = false);
    }
}