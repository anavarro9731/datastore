        namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemThatDoesNotExist
    {
        private Car activeCarFromDatabase;

        async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemThatDoesNotExist));

            // When
            this.activeCarFromDatabase = await testHarness.DataStore.ReadActiveById<Car>(Guid.NewGuid());
        }

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(this.activeCarFromDatabase);
        }
    }
}