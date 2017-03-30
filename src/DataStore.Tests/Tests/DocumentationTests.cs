using System;
using System.Linq;
using DataStore.Impl.DocumentDb;
using DataStore.Models.Messages.Events;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using Xunit;

namespace DataStore.Tests.Tests
{
    [Collection(TestCollections.DocumentationTests)]
    public class DocumentationTests
    {
        [Fact]
        public async void CanUpdateCar()
        {
            var documentRepository = new InMemoryDocumentRepository();
            var inMemoryDb = documentRepository.Aggregates;
            var dataStore = new DataStore(documentRepository);

            var carId = Guid.NewGuid();

            //Given
            inMemoryDb.Add(new Car
            {
                id = carId,
                Make = "Toyota"
            });

            //When
            await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            await dataStore.CommitChanges();

            //Then 

            //We have a AggregateUpdated event
            Assert.NotNull(dataStore.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));

            //The underlying database has changed
            Assert.Equal("Ford", inMemoryDb.OfType<Car>().Single(car => car.id == carId).Make);

            //The dataStore reads the changes correctly
            Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
        }

        [Fact]
        public async void WhenUpdateCarButDontCommitChangesOnlyTheLocalCacheIsAffected()
        {
            var documentRepository = new InMemoryDocumentRepository();
            var inMemoryDb = documentRepository.Aggregates;
            var dataStore = new DataStore(documentRepository);

            var carId = Guid.NewGuid();

            //Given
            inMemoryDb.Add(new Car
            {
                id = carId,
                Make = "Toyota"
            });

            //When
            await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            //await dataStore.CommitChanges(); don't commit

            //Then 

            //We have a AggregateUpdated event
            Assert.NotNull(dataStore.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));

            //The underlying database has NOT changed
            Assert.Equal("Toyota", inMemoryDb.OfType<Car>().Single(car => car.id == carId).Make);

            //The DataStore instance picks up the change, because it has applied
            //all changes made during this session.
            Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}