namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Collections.Generic;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate
    {
        public WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteHardWhereAndNoItemsMatchThePredicate));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly IEnumerable<Car> result;

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(result);
        }
    }
}