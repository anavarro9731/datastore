namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist
    {
        private Car result;

         async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardByIdOnAnItemThatDoesNotExist));

            //When
            this.result = await testHarness.DataStore.DeleteHardById<Car>(Guid.NewGuid());
            await testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(this.result);
        }
    }
}