using System;
using System.Collections.Generic;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
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
        public void ItShouldReturnOnlyOneItem()
        {
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(1, result.Count());
        }
    }
}