using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
{
    [Collection(TestCollections.RunSerially)]
    public class DataStoreUpdateCapabilitiesTests
    {
        [Fact]
        public async void WhenCallingCanUpdateWhereWithoutCommitting_ItShouldOnlyMakeTheChangesLocally()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCanUpdateWhereWithoutCommitting_ItShouldOnlyMakeTheChangesLocally));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdate_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterUpdate_ItShouldPersistChangesToTheDatabase));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            await testHarness.DataStore.Update(existingCarFromDb);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdateById_ItShouldPersistTheChangesToTheDatabase()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterUpdateById_ItShouldPersistTheChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdateWhere_ItShouldPersistTheChangesToTheDatabase()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterUpdateWhere_ItShouldPersistTheChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingUpdateByIdWithoutCommitting_ItShouldOnlyMakeTheChangesLocally()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateByIdWithoutCommitting_ItShouldOnlyMakeTheChangesLocally));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingUpdateWhereWithoutCommitting_ItShouldConsiderPreviousChanges()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateWhereWithoutCommitting_ItShouldConsiderPreviousChanges));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });
            await testHarness.DataStore.DeleteHardById<Car>(carId);

            //When
            var results = await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");

            //Then
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));

            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            Assert.Equal(results.Count(), 0); //there nothing should have been updated because it was already deleted.
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
        }

        [Fact]
        public async void WhenCallingUpdateWithoutCommitting_ItShouldOnlyMakeTheChangesLocally()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateWithoutCommitting_ItShouldOnlyMakeTheChangesLocally));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            await testHarness.DataStore.Update(existingCarFromDb);

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}