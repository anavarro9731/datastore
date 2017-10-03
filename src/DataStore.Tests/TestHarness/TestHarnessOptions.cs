namespace DataStore.Tests.TestHarness
{
    using global::DataStore.Impl.DocumentDb.Config;

    public class TestHarnessOptions
    {
        public DocDbCollectionSettings CollectionSettings { get; private set; }

        public static TestHarnessOptions Create(DocDbCollectionSettings collectionSettings)
        {
            return new TestHarnessOptions
            {
                CollectionSettings = collectionSettings
            };
        }
    }
}