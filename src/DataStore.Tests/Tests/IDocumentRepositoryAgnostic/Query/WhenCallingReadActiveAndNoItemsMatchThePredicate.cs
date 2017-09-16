using System;
using System.Collections.Generic;
using System.Linq;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    public class WhenCallingReadActiveAndNoItemsMatchThePredicate
    {
        public WhenCallingReadActiveAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveAndNoItemsMatchThePredicate));

            // When
            carsFromDatabase = testHarness.DataStore.ReadActive<Car>(car => car.id == Guid.NewGuid())
                .Result;
        }

        private readonly IEnumerable<Car> carsFromDatabase;

        [Fact]
        public void ItShouldReturnAnEmptyResultset()
        {
            Assert.Empty(carsFromDatabase);
        }
    }
}