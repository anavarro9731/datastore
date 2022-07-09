namespace DataStore.Tests.Tests.Documentation
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using Xunit;

    public class DocumentationTests
    {
        [Fact]
        public async void CanUpdateCar()
        {
            var documentRepository = new InMemoryDocumentRepository();
            var dataStore = new DataStore(documentRepository);

            var carId = Guid.NewGuid();

            //Given
            await dataStore.Create(
                new Car
                {
                    id = carId, Make = "Toyota"
                });
            await dataStore.CommitChanges();

            //When
            await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            await dataStore.CommitChanges();

            //Then 

            //We have executed an update operation
            Assert.NotNull(dataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));

            //We have no queued update operations
            Assert.Null(dataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //The dataStore reads the changes correctly
            Assert.Equal("Ford", (await dataStore.ReadActiveById<Car>(carId)).Make);
        }

        [Fact]
        public async void WhenUpdateCarButDontCommitChangesOnlyTheLocalCacheIsAffected()
        {
            var documentRepository = new InMemoryDocumentRepository();
            var dataStore = new DataStore(documentRepository);

            var carId = Guid.NewGuid();

            //Given
            await dataStore.Create(
                new Car
                {
                    id = carId, Make = "Toyota"
                });
            await dataStore.CommitChanges();

            //When
            await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
            //await dataStore.CommitChanges(); don't commit

            //Then 

            //We have a queued update operation
            Assert.NotNull(dataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //We have not execute any update operations
            Assert.Null(dataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));

            //The underlying database has NOT changed
            var partitionKeyValues = new PartitionKeyValues($"{typeof(Car).FullName}_{carId}");
            var car = documentRepository.AggregatesByLogicalPartition[partitionKeyValues].Single().As<Car>();
            Assert.Equal("Toyota", car.Make);

            //The DataStore instance picks up the change, because it has applied
            //all the previous changes made during this session to any query.
            Assert.Equal("Ford", (await dataStore.ReadActiveById<Car>(carId)).Make);
        }
    }
}