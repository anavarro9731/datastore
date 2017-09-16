using System.Collections.Generic;
using System.Linq;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    public class WhenCallingReadAndNoItemsMatchThePredicate
    {
        public WhenCallingReadAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadAndNoItemsMatchThePredicate));

            // When
            carsFromDatabase = testHarness.DataStore.Read<Car>(car => car.Make == "None").Result;
        }

        private readonly IEnumerable<Car> carsFromDatabase;

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(carsFromDatabase);
        }
    }
}