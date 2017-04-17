namespace DataStore.Tests.Tests.AzureOnly
{
    using TestHarness;
    using Xunit;

    public class DeleteAllCollections
    {
        
        [Fact(Skip = "Used only for maintenance")]
        //[Fact]
        public void CanDeleteAllCollections()
        {
            var harness = TestHarnessFunctions.GetDocumentDbTestHarness(nameof(CanDeleteAllCollections)) as DocumentDbTestHarness;
            harness.RemoveAllCollections();
        }
    }
}