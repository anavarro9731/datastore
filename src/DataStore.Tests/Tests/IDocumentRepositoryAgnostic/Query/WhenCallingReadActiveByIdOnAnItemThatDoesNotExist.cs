using System;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    public class WhenCallingReadActiveByIdOnAnItemThatDoesNotExist
    {
        public WhenCallingReadActiveByIdOnAnItemThatDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnItemThatDoesNotExist));

            // When
            activeCarFromDatabase = testHarness.DataStore.ReadActiveById<Car>(Guid.NewGuid()).Result;
        }

        private readonly Car activeCarFromDatabase;

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(activeCarFromDatabase);
        }
    }
}