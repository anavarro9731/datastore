namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveAndNoItemsMatchThePredicate
    {
        private  IEnumerable<Car> carsFromDatabase;

        async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActiveAndNoItemsMatchThePredicate));

            // When
            this.carsFromDatabase = (await testHarness.DataStore.ReadActive<Car>(car => car.id == Guid.NewGuid()));
        }

        [Fact]
        public async void ItShouldReturnAnEmptyResultset()
        {
            await Setup();
            Assert.Empty(this.carsFromDatabase);
        }
    }
}