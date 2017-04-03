namespace DataStore.Interfaces
{
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreCreateCapabilitiesScoped<T> where T : IAggregate, new()
    {
        Task<T> Create(T model, bool readOnly = false);
    }
}