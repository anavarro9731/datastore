namespace DataStore.Tests.Tests.Read
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadAndNoItemsMatchThePredicate
    {
        private IEnumerable<Car> carsFromDatabase;

        [Fact]
        public async void ItShouldReturnAnEmptyList()
        {
            await Setup();
            Assert.Empty(this.carsFromDatabase);
        }

        private async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadAndNoItemsMatchThePredicate));

            // When
            this.carsFromDatabase = await testHarness.DataStore.Read<Car>(car => car.Make == "None");
        }
    }
}