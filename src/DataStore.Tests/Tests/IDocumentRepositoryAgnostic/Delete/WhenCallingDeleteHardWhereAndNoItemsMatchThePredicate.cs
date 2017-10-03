namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
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
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            //When
            this.result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(this.result);
        }
    }
}