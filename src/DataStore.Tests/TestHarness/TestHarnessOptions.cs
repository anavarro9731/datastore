using DataStore.Impl.DocumentDb.Config;

namespace DataStore.Tests.TestHarness
{
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