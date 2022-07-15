namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingReadActiveAndNoItemsMatchThePredicate
    {
        private IEnumerable<Car> carsFromDatabase;

        [Fact]
        public async void ItShouldReturnAnEmptyResultset()
        {
            await Setup();
            Assert.Empty(this.carsFromDatabase);
        }

        private async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActiveAndNoItemsMatchThePredicate));

            // When
            this.carsFromDatabase = await testHarness.DataStore.ReadActive<Car>(car => car.id == Guid.NewGuid());
        }
    }
}