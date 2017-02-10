using DataStore.Models.Config;

namespace Tests.TestHarness
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