namespace DataStore.Interfaces
{
    using System.Threading.Tasks;

    public interface IDatabaseUtilities
    {
        Task CreateDatabaseIfNotExists(IDatabaseSettings cosmosStoreSettings);

        Task ResetDatabase(IDatabaseSettings cosmosStoreSettings);
    }
}