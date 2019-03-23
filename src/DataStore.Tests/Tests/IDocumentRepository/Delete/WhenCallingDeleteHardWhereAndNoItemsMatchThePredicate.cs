namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate
    {
        private readonly IEnumerable<Car> result;

        public WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(
                new Car
                {
                    Id = carId,
                    Make = "Volvo"
                });

            //When
            this.result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.Id == Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(this.result);
        }
    }
}