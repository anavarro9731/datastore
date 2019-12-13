namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate
    {
        private IEnumerable<Car> result;

        async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate));

            //When
            this.result = await testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == Guid.NewGuid());
            await testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldReturnAnEmptyList()
        {
            await Setup();
            Assert.Empty(this.result);
        }
    }
}