namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereOnAnItemDeletedInThisSession
    {
        public WhenCallingUpdateWhereOnAnItemDeletedInThisSession()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWhereOnAnItemDeletedInThisSession));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });
            testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            //When
            results = testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford").Result;
        }

        private readonly ITestHarness testHarness;
        private readonly IEnumerable<Car> results;
        private readonly Guid carId;

        [Fact]
        public void ItShouldConsiderThePreviousDeleteInAnyFutureQueriesInSession()
        {
            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //nothing should have been updated because it was already deleted.
            Assert.Equal(0, results.Count());
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
        }

        [Fact]
        public void ItShouldDeleteTheItemInSession()
        {
            Assert.NotNull(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
        }
    }
}