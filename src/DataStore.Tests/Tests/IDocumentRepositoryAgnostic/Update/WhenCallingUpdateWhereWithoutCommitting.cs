using System;
using System.Collections.Generic;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateWhereWithoutCommitting
    {
        public WhenCallingUpdateWhereWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWhereWithoutCommitting));

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
        public void ItShouldConsiderPreviousChanges()
        {
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(results.Count(), 0); //there nothing should have been updated because it was already deleted.
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
        }
    }
}

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
{
    public class WhenCallingUpdateWhereWithoutCommitting
    {
        public WhenCallingUpdateWhereWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWhereWithoutCommitting));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford").Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public void ItShouldOnlyMakeTheChangesInSession()
        {
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}