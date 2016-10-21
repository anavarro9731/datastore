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
    public class DataStoreQueryCapabilitiesTests
    {
        [Fact]
        public async void CanRead()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanRead));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            // When
            var carFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))).Single();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregatesQueried<Car>));
            Assert.Equal("Volvo", carFromDatabase.Make);
        }

        [Fact]
        public async void CanReadActive()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadActive));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Active = true,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            await testHarness.AddToDatabase(activeExistingCar);
            await testHarness.AddToDatabase(inactiveExistingCar);

            // When
            var activeCarFromDatabase = (await testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == activeCarId))).SingleOrDefault();
            var inactiveCarFromDatabase = (await testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == inactiveCarId))).SingleOrDefault();

            //Then
            Assert.Equal(2, testHarness.Events.Count(e => e is AggregatesQueried<Car>));
            Assert.Equal("Volvo", activeCarFromDatabase.Make);
            Assert.Null(inactiveCarFromDatabase);
        }

        [Fact]
        public async void CanReadById()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadById));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            // When
            var document = await testHarness.DataStore.ReadById(carId);
            var carFromDatabase = (Car)(dynamic)document;

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateQueriedById));
            Assert.Equal("Volvo", carFromDatabase.Make);
        }

        [Fact]
        public async void CanReadActiveById()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadActiveById));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Active = true,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            await testHarness.AddToDatabase(activeExistingCar);
            await testHarness.AddToDatabase(inactiveExistingCar);

            // When
            var activeCarFromDatabase = await testHarness.DataStore.ReadActiveById<Car>(activeCarId);
            var exception = Assert.ThrowsAsync<Exception>(async () => await testHarness.DataStore.ReadActiveById<Car>(inactiveCarId));

            //Then
            Assert.Equal(2, testHarness.Events.Count(e => e is AggregatesQueried<Car>));
            Assert.Equal("Volvo", activeCarFromDatabase.Make);
            Assert.NotNull(exception);

        }

        [Fact]
        public async void CanReadAndApplyUncommittedUpdateChanges()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadAndApplyUncommittedUpdateChanges));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            await testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford");

            // When
            var carFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))).Single();

            //Then
            Assert.NotNull(testHarness.Events.All(e => e is AggregatesQueried<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", carFromDatabase.Make);
        }

        [Fact]
        public async void CanReadAndApplyUncommittedHardDeleteChanges()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadAndApplyUncommittedHardDeleteChanges));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            await testHarness.DataStore.DeleteHardById<Car>(carId);

            // When
            var readCarFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))).SingleOrDefault();

            //Then
            Assert.NotNull(testHarness.Events.All(e => e is AggregatesQueried<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Null(readCarFromDatabase);
        }

        [Fact]
        public async void CanReadAndApplyUncommittedSoftDeleteChanges()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadAndApplyUncommittedSoftDeleteChanges));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            await testHarness.DataStore.DeleteSoftById<Car>(carId);

            // When
            var readCarFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))).SingleOrDefault();

            //Then
            Assert.NotNull(testHarness.Events.All(e => e is AggregatesQueried<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.NotNull(readCarFromDatabase);
        }

        [Fact]
        public async void CanReadAndApplyUncommittedCreateChanges()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadAndApplyUncommittedCreateChanges));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            //When
            await testHarness.DataStore.Create(new Car()
            {
                id = Guid.NewGuid(),
                Active = true,
                Make = "Ford"
            });

            // When
            var readCarFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.id == carId))).Count();
            var readActiveCarFromDatabase = (await testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == carId))).Count();

            //Then
            Assert.NotNull(testHarness.Events.All(e => e is AggregatesQueried<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Equal(2, readCarFromDatabase);
            Assert.Equal(2, readActiveCarFromDatabase);
        }



        [Fact]
        public async void CanReadCommittedOfDifferentType()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadCommittedOfDifferentType));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            // When
            var transformedCar = (await testHarness.DataStore.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId).Select(c => new { c.id, c.Make }))).Single();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e.TypeName == transformedCar.GetType().FullName));
            Assert.Equal("Volvo", transformedCar.Make);
        }

        [Fact]
        public async void CanReadCommittedOfSameType()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanReadCommittedOfSameType));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            // When
            var carFromDatabase = (await testHarness.DataStore.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId))).Single();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is TransformationQueried<Car>));
            Assert.Equal("Volvo", carFromDatabase.Make);
        }
    }
}