namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingUpdateGiven1Of2ItemsExists
    {
        public WhenCallingUpdateGiven1Of2ItemsExists()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateGiven1Of2ItemsExists));

            var volvoId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = volvoId,
                Make = "Volvo"
            });

            var fordId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = fordId,
                Make = "Ford"
            });

            //When
            result = testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "Volvo", car => { }).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly IEnumerable<Car> result;
        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldExecuteOnlyOneUpdate()
        {
            Assert.Equal(1, testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnOnlyOneItem()
        {
            Assert.Equal(1, result.Count());
        }
    }
}