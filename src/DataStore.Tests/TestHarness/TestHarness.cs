namespace DataStore.Tests.TestHarness
{
    using System.Threading.Tasks;
    using global::DataStore.Providers.CosmosDb.ExtremeConfigAwait;

    public static class TestHarness
    {
        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            

            return CosmosDbTestHarness.Create(testName, dataStoreOptions).Result;
            //return InMemoryTestHarness.Create(dataStoreOptions);
        }
    }
}