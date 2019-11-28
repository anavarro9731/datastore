namespace DataStore.Tests.TestHarness
{
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Providers.CosmosDb;

    public static class TestHarness
    {
        private static readonly TestHarnessBackingStore BackingStore = TestHarnessBackingStore.CosmosDb;

        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            switch (BackingStore)
            {
                case TestHarnessBackingStore.CosmosDb:

                    return Task.Run(
                        async () => await CosmosDbTestHarness.Create(
                                        testName,
                                        new DataStore(
                                            new CosmosDbRepository(CosmosDbTestHarness.GetCosmosStoreSettings(testName)), dataStoreOptions: dataStoreOptions)).ConfigureAwait(false)).Result;

                case TestHarnessBackingStore.InMemory:
                default:
                    return InMemoryTestHarness.Create(dataStoreOptions);
            }
        }

        private enum TestHarnessBackingStore
        {
            InMemory,

            CosmosDb
        }
    }
}