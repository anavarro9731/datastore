namespace DataStore.Tests.TestHarness
{
    using global::DataStore.Models.Config;

    public class TestHarnessOptions
    {
        public static TestHarnessOptions Create(DocDbCollectionSettings collectionSettings)
        {
            return new TestHarnessOptions()
            {
                CollectionSettings = collectionSettings
            };
        }

        public DocDbCollectionSettings CollectionSettings { get; private set; }
    }
}