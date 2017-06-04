using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenCallingUpdateWithoutCommitting
    {
        public WhenCallingUpdateWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingUpdateWithoutCommitting));

            carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            testHarness.DataStore.Update(existingCarFromDb).Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public void ItShouldOnlyMakeTheChangesInSession()
        {
            Assert.Null(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}