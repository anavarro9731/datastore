namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Initialise
{
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenAccessingTheDatabase
    {
        public WhenAccessingTheDatabase()
        {
            //When
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenAccessingTheDatabase));
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldHaveCreatedTheCollection()
        {
            //Then
            Assert.Equal(0, testHarness.QueryDatabase<Car>(query => query.Select(x => x)).Count());
        }
    }
}