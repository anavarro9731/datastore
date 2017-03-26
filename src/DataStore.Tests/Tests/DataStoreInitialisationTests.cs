namespace DataStore.Tests.Tests
{
    using System.Linq;
    using Constants;
    using Models;
    using TestHarness;
    using Xunit;

    [Collection(TestCollections.DataStoreTestCollection)]
    public class DataStoreInitialisationTests
    {
        [Fact]
        public void Integration_WhenAccessTheDatabase_ItShouldHaveCreatedTheCollection()
        {
            //When
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(Integration_WhenAccessTheDatabase_ItShouldHaveCreatedTheCollection));

            //Then
            Assert.Equal(0, testHarness.QueryDatabase<Car>(query => query.Select(x => x)).Result.Count());
        }
    }
}