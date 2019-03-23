namespace DataStore.Tests.TestHarness
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Providers.CosmosDb;

    public static class TestHarness
    {
        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            return CosmosDbTestHarness.Create(
                testName,
                new DataStore(
                    new CosmosDbRepository(CosmosDbTestHarness.GetCosmosStoreSettings(testName)),
                    dataStoreOptions: dataStoreOptions)).Result;
            
            //return InMemoryTestHarness.Create(dataStoreOptions);
        }
    }
}