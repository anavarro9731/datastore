namespace DataStore.Interfaces
{
    #region

    using System.Threading.Tasks;

    #endregion

    public interface IDatabaseUtilities
    {
        Task CreateDatabaseIfNotExists(IDatabaseSettings cosmosStoreSettings, bool useSharedThroughput);

        Task ResetDatabase(IDatabaseSettings cosmosStoreSettings, bool useSharedThroughput);
    }
}