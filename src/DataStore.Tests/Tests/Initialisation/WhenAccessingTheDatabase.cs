namespace DataStore.Tests.Tests.Initialisation
{
    #region

    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

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
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>(query => query.Select(x => x)));
        }
    }
}