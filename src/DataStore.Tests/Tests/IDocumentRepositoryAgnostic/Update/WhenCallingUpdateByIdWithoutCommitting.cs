using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateByIdWithoutCommitting
    {
        public WhenCallingUpdateByIdWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateByIdWithoutCommitting));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford").Wait();
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