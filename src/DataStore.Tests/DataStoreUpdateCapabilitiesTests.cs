using System;
using System.Linq;
using DataStore.DataAccess.Models.Messages.Events;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using Xunit;
using static DataStore.Tests.TestFunctions;

namespace DataStore.Tests
{
    [Collection(TestCollection.DataStoreTestCollection)]
    public class DataStoreUpdateCapabilitiesTests
    {
        [Fact]
        public async void CanUpdateWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateWithoutCommit));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            await testHarness.DataStore.Update(existingCarFromDb);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void CanUpdateWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateWithCommit));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";
            await testHarness.DataStore.Update(existingCarFromDb);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void CanUpdateByIdWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateByIdWithoutCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void CanUpdateByIdWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateByIdWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void CanUpdateWhereWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateWhereWithoutCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }


        [Fact]
        public async void UpdateWhereWithoutCommitConsidersPreviousChanges()
        {
            // Given
            var testHarness = GetTestHarness(nameof(UpdateWhereWithoutCommitConsidersPreviousChanges));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });
            await testHarness.DataStore.DeleteHardById<Car>(carId);

            //When
            var results = await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");

            //Then
            Assert.Equal(results.Count(), 0); //there nothing should have been updated because it was already deleted.
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            
        }

        [Fact]
        public async void CanUpdateWhereWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanUpdateWhereWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.UpdateWhere<Car>(car => car.id == carId, car => car.Make = "Ford");
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}
