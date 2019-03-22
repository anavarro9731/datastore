namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System.Collections.Generic;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadAndNoItemsMatchThePredicate
    {
        private readonly IEnumerable<Car> carsFromDatabase;

        public WhenCallingReadAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadAndNoItemsMatchThePredicate));

            // When
            this.carsFromDatabase = testHarness.DataStore.Read<Car>(car => car.Make == "None").Result;
        }

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(this.carsFromDatabase);
        }
    }
}