namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate
    {
        private readonly IEnumerable<Car> result;

        public WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteSoftWhereAndNoItemsMatchThePredicate));

            //When
            this.result = testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == Guid.NewGuid()).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.Empty(this.result);
        }
    }
}