namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Collections.Generic;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate
    {
        public WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate));

            //When
            result = testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == Guid.NewGuid()).Result;
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