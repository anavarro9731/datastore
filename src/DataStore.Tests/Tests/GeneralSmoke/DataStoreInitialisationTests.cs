using System.Linq;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.GeneralSmoke
{
    [Collection(TestCollections.CrudCapabilityTests)]
    public class DataStoreInitialisationTests
    {
        [Fact]
        public void Integration_WhenAccessTheDatabase_ItShouldHaveCreatedTheCollection()
        {
            //When
            var testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(Integration_WhenAccessTheDatabase_ItShouldHaveCreatedTheCollection));

            //Then
            Assert.Equal(0, testHarness.QueryDatabase<Car>(query => query.Select(x => x)).Result.Count());
        }
    }
}