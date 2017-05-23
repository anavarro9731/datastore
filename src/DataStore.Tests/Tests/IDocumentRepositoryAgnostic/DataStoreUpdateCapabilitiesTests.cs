namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
{
    using System;
    using System.Linq;
    using Constants;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    [Collection(TestCollections.RunSerially)]
    public class DataStoreUpdateCapabilitiesTests
    {
        [Fact]
        public async void WhenCallingCommitAfterUpdate_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterUpdate_ItShouldPersistChangesToTheDatabase));

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
        public async void WhenCallingCommitAfterUpdatingTwiceInOneSession_ItShouldPersistTheLastChangeToTheDatabase()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterUpdatingTwiceInOneSession_ItShouldPersistTheLastChangeToTheDatabase));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            await testHarness.DataStore.UpdateById<Car>(carId, c => c.Make = "Toyota");
            await testHarness.DataStore.UpdateById<Car>(carId, c => c.Make = "Honda");

            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.Equal(2, testHarness.Operations.Count(e => e is UpdateOperation<Car>));
            Assert.Equal(2, testHarness.QueuedWriteOperations.Count(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Honda", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Honda", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdateById_ItShouldPersistTheChangesToTheDatabase()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateById_ItShouldPersistTheChangesToTheDatabase));

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
        public async void WhenCallingCommitAfterUpdateByIdWhenItemDoesNotExist_ItShouldReturnNull()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateByIdWhenItemDoesNotExist_ItShouldReturnNull));

            var carId = Guid.NewGuid();

            //When
            var result = await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Whatever");
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Null(result);
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdateWhenItemDoesNotExist_ItShouldReturnNull()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateWhenItemDoesNotExist_ItShouldReturnNull));

            //When
            var result = await testHarness.DataStore.Update(new Car());
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Null(result);
        }


        [Fact]
        public async void WhenCallingCommitAfterUpdateWhere_ItShouldPersistTheChangesToTheDatabase()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateWhere_ItShouldPersistTheChangesToTheDatabase));

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
        public async void WhenCallingCommitAfterUpdateWhere1of2ItemsExists_ItShouldReturnOnlyOneItem()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateWhere1of2ItemsExists_ItShouldReturnOnlyOneItem));

            var volvoId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = volvoId,
                Make = "Volvo"
            });

            var fordId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = fordId,
                Make = "Ford"
            });

            //When
            var result = await testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "Volvo", car => { });
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public async void WhenCallingCommitAfterUpdateWhereWhenNoItemsExist_ItShouldReturnNoResults()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingCommitAfterUpdateWhereWhenNoItemsExist_ItShouldReturnNoResults));

            //When
            var result = await testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "DoesNotExist", car => { });
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Empty(result);
        }


        [Fact]
        public async void WhenCallingUpdateByIdWithoutCommitting_ItShouldOnlyMakeTheChangesInSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingUpdateByIdWithoutCommitting_ItShouldOnlyMakeTheChangesInSession));

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
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingUpdateWhereWithoutCommitting_ItShouldConsiderPreviousChanges()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingUpdateWhereWithoutCommitting_ItShouldConsiderPreviousChanges));

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
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
        }

        [Fact]
        public async void WhenCallingUpdateWhereWithoutCommitting_ItShouldOnlyMakeTheChangesInSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingUpdateWhereWithoutCommitting_ItShouldOnlyMakeTheChangesInSession));

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
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenCallingUpdateWithoutCommitting_ItShouldOnlyMakeTheChangesInSession()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenCallingUpdateWithoutCommitting_ItShouldOnlyMakeTheChangesInSession));

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
            Assert.Equal("Volvo",
                testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenChangingTheItemsReturnedFromUpdate_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnedFromUpdate_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            var result = await testHarness.DataStore.Update(existingCarFromDb);
            //When
            result.id = Guid.NewGuid();
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }


        [Fact]
        public async void WhenChangingTheItemsReturnedFromUpdateById_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnedFromUpdateById_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var result = await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            //When
            result.id = Guid.NewGuid();
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenChangingTheItemsReturnedFromUpdateWhere_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            // Given
            var testHarness =
                TestHarnessFunctions.GetTestHarness(
                    nameof(WhenChangingTheItemsReturnedFromUpdateWhere_ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var results = await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");

            //When
            foreach (var car in results)
                car.id = Guid.NewGuid();
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}