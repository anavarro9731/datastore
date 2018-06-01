namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateGiven1Of2ItemsExists
    {
        private readonly IEnumerable<Car> result;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateGiven1Of2ItemsExists()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateGiven1Of2ItemsExists));

            var volvoId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = volvoId,
                    Make = "Volvo"
                });

            var fordId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = fordId,
                    Make = "Ford"
                });

            //When
            this.result = this.testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "Volvo", car => { }).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldExecuteOnlyOneUpdate()
        {
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnOnlyOneItem()
        {
            Assert.Single(this.result);
        }
    }
}