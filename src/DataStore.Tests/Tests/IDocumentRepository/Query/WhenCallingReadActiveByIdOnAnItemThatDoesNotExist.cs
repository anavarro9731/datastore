namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemThatDoesNotExist
    {
        private readonly Car activeCarFromDatabase;

        public WhenCallingReadActiveByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemThatDoesNotExist));

            // When
            this.activeCarFromDatabase = testHarness.DataStore.ReadActiveById<Car>(Guid.NewGuid()).Result;
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.activeCarFromDatabase);
        }
    }
}