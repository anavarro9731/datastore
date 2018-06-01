namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereOnAnItemDeletedInThisSession
    {
        private readonly Guid carId;

        private readonly IEnumerable<Car> results;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateWhereOnAnItemDeletedInThisSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateWhereOnAnItemDeletedInThisSession));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });
            this.testHarness.DataStore.DeleteHardById<Car>(this.carId).Wait();

            //When
            this.results = this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford").Result;
        }

        [Fact]
        public void ItShouldConsiderThePreviousDeleteInAnyFutureQueriesInSession()
        {
            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //nothing should have been updated because it was already deleted.
            Assert.Empty(this.results);
            Assert.Equal("Volvo", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
        }

        [Fact]
        public void ItShouldDeleteTheItemInSession()
        {
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
        }
    }
}