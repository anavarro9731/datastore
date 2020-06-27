namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate
    {
        private IEnumerable<Car> result;

        [Fact]
        public async void ItShouldReturnAnEmptyList()
        {
            await Setup();
            Assert.Empty(this.result);
        }

        private async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate));

            var carId = Guid.NewGuid();
            testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = carId, Make = "Volvo"
                });

            //When
            this.result = await testHarness.DataStore.DeleteWhere<Car>(car => car.id == Guid.NewGuid(), o => o.Permanently());
            await testHarness.DataStore.CommitChanges();
        }
    }
}