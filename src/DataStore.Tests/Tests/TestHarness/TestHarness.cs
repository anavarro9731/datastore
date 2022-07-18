namespace DataStore.Tests.Tests.TestHarness
{
    #region

    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Options;
    using global::DataStore.Providers.CosmosDb;

    #endregion

    public static class TestHarness
    {
        private static readonly TestHarnessBackingStore BackingStore = TestHarnessBackingStore.CosmosDb;

        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null, bool useHierarchicalPartitionKey = true)
        {
            switch (BackingStore)
            {
                case TestHarnessBackingStore.CosmosDb:

                    return Task.Run(
                        async () => await CosmosDbTestHarness.Create(
                                                                 testName,
                                                                 (options, settings) => new DataStore(settings.CreateRepository(), dataStoreOptions: options),
                                                                 dataStoreOptions, useHierarchicalPartitionKey)
                                                             .ConfigureAwait(false)).Result;

                case TestHarnessBackingStore.InMemory:
                default:
                    return InMemoryTestHarness.Create(useHierarchicalPartitionKey, dataStoreOptions);
            }
        }

        private enum TestHarnessBackingStore
        {
            InMemory,

            CosmosDb
        }
    }
}
