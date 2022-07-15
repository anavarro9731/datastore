namespace DataStore.Tests.Tests.Delete
{
    #region

    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist
    {
        private Car result;

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(this.result);
        }

        private async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoftByIdOnAnItemThatDoesNotExist));

            //When
            this.result = await testHarness.DataStore.DeleteById<Car>(Guid.NewGuid());
            await testHarness.DataStore.CommitChanges();
        }
    }
}