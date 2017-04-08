using DataStore.Impl.DocumentDb.Config;

namespace DataStore.Tests.TestHarness
{
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