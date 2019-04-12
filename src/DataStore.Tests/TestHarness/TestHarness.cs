namespace DataStore.Tests.TestHarness
{
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Providers.CosmosDb;

    public static class TestHarness
    {
        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            return Task.Run(async () => await CosmosDbTestHarness.Create(
                testName,
                new DataStore(
                    new CosmosDbRepository(CosmosDbTestHarness.GetCosmosStoreSettings(testName)),
                    dataStoreOptions: dataStoreOptions)).ConfigureAwait(false)).Result;
                    
            //return InMemoryTestHarness.Create(dataStoreOptions);
        }
    }
}