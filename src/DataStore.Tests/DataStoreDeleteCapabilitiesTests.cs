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
    public class DataStoreDeleteCapabilitiesTests
    {
        [Fact]
        public async void CanDeleteHardByIdWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteHardByIdWithoutCommit));
            
            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteHardById<Car>(carId);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateHardDeleted<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteHardByIdWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteHardByIdWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteHardById<Car>(carId);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateHardDeleted<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteHardWhereWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteHardWhereWithoutCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateHardDeleted<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteHardWhereWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteHardWhereWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateHardDeleted<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteSoftByIdWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteSoftByIdWithoutCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftById<Car>(carId);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateSoftDeleted<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteSoftByIdWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteSoftByIdWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftById<Car>(carId);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateSoftDeleted<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars =>cars.Where(car => car.id == carId)).Result.Single().Active);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteSoftWhereWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteSoftWhereWithoutCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateSoftDeleted<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>().Result);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }

        [Fact]
        public async void CanDeleteSoftWhereWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanDeleteSoftWhereWithCommit));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            });

            //When
            await testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateSoftDeleted<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Active);
            Assert.Empty(testHarness.DataStore.ReadActive<Car>(car => car).Result);
            Assert.NotEmpty(testHarness.DataStore.Read<Car>(car => car).Result);
        }
    }
}