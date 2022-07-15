namespace DataStore.Interfaces
{
    #region

    using System.Threading.Tasks;

    #endregion

    public interface IDatabaseUtilities
    {
        Task CreateDatabaseIfNotExists(IDatabaseSettings cosmosStoreSettings);

        Task ResetDatabase(IDatabaseSettings cosmosStoreSettings);
    }
}