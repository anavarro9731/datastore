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
        public async void WhenCallingTheDeleteHardByIdWithoutCommitting_ItShouldOnlyMakeChangesLocally()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingTheDeleteHardByIdWithoutCommitting_ItShouldOnlyMakeChangesLocally));
            
            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingCommitAfterDeleteHardById_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingCommitAfterDeleteHardById_ItShouldPersistChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingTheDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesLocally()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingTheDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesLocally));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingCommitAfterDeleteHardWhere_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingCommitAfterDeleteHardWhere_ItShouldPersistChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingDeleteSoftByIdWithoutCommitting_ItShouldOnlyMakeChangesLocally()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingDeleteSoftByIdWithoutCommitting_ItShouldOnlyMakeChangesLocally));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingCommitAfterDeleteSoftById_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingCommitAfterDeleteSoftById_ItShouldPersistChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingDeleteSoftWhereWithoutCommitting_ItShouldOnlyMakeTheChangesLocally()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingDeleteSoftWhereWithoutCommitting_ItShouldOnlyMakeTheChangesLocally));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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
        public async void WhenCallingCommitAfterDeleteSoftWhere_ItShouldPersistTheChangesToTheDatabase()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingCommitAfterDeleteSoftWhere_ItShouldPersistTheChangesToTheDatabase));

            var carId = Guid.NewGuid();
            await testHarness.AddToDatabase(new Car
            {
                id = carId,
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