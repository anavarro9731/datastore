namespace DataStore.Tests.Tests.IDocumentRepository.Initialise
{
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenAccessingTheDatabase
    {
        private readonly ITestHarness testHarness;

        public WhenAccessingTheDatabase()
        {
            //When
            this.testHarness = TestHarness.Create(nameof(WhenAccessingTheDatabase));
        }

        [Fact]
        public void ItShouldHaveCreatedTheCollection()
        {
            //Then
            Assert.Empty(this.testHarness.QueryDatabase<Car>(query => query.Select(x => x)));
        }
    }
}