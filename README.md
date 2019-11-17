# DataStore

A Document-Centric Data Access Framework for Azure CosmosDb (DocumentDB client)

## Overview

DataStore is an easy-to-use, data-access framework, which maps POCO C# classes to documents. 

Currently the only supported database is Azure CosmosDb (via DataStore.Providers.CosmosDb package).

It supports basic CRUD operations on any C# object, with some additional features such as:


* Generic Repository with IQueryable<T> support for LINQ queries against objects and their children 
	(limited by CosmosDB .NET client support, does include paging with continuation tokens)
* Increased transactional consistency with re-usable Sessions (i.e. Unit of Work pattern)
* Limited Partitioned Collection Support (partition on Id or ClassName)
* In-memory database, and event history for testing
(e.g. DataStore.ExecutedOperations.Where(o => o...))
* Profiling (e.g. Duration and Query Cost in Request Units)
* Automatic Id and timestamp management of object hierarchies 
* Automatic retries of queries when limits are exceeded
* Session-based vs Direct-to-Database views of data. 
Using a feature called "Event Replay" queries by default will reflect all previous changes made in the session. 
This is can be circumnvented by choosing to use the "WithoutEventReplay" option on any call.
Some features such as Continue/Take, and OrderBy/ThenBy are available only WithoutEventReplay for obvious reasons.

DataStore is built with .NET Core but it is compatible with the all NetStandard2.0 platforms including .NET Framework 4.6.1 and does not require .NET Core.

## Roadmap

* Better documentation of API features
* Optimistic Concurrency Support
* Document-level security

## Usage

Import the Nuget Package *DataStore*.
Import the Nuget Package *DataStore.Providers.CosmosDb*

Create a C# class which inherits `DataStore.Interfaces.LowLevel.Aggregate`.
```
class Car : Aggregate {
	public string Make { get; set; }
	public string Model { get; set; }
}
```
Create a new `DataStore` object.
```
var d = new DataStore.DataStore(new CosmosDbRepository(
			new CosmosSettings(
				authorizationKey, 
				databaseName, 
				endpointUrl)
			));
```
##### Save it to DocumentDb

`var car = d.Create(new Car() { Make = "Toyota", Model = "Corolla"});`

##### Update it 

`d.UpdateById<Car>(car.id, (car) => car.Model = "Celica");`

or
```
car.Model = "Celica";
d.Update(car);
```
> NOTE: The Update() method does not persist the object you pass it directly.
> Instead it copies matching properties from the instance you pass it to an instance 
> it de-serializes from the database. In this way reserved properties are excluded along
> with any properties which may not be present on the current class definition (e.g. if you
> pass a derived class with additional properties)

##### Delete It

`d.DeleteSoftById<Car>(car.Id);`

> There are 2 delete approaches; Hard and Soft. Hard deletes will remove the document entirely.
> Soft deletes will only toggle the document's *Active* flag. The Active flag is filtered on
> by various Read methods (e.g. ReadActive(), ReadActiveById())

##### Retrieve It

`var toyotaCars = d.Read<Car>(c => c.Model = "Toyota"));`

or

`var myToyota = d.ReadActiveById<Car>(car.id);`

See IDataStoreQueryCapabilities.cs for the full list of supported methods.

#### Transactions

Operations are not committed by default.
They are queued as events in the EventAggregator object.
Calling DataStore.CommitChanges() will commit these events to the database.

```    
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

            //We have executed an update operation
            Assert.NotNull(dataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));

            //We have no queued update operations
            Assert.Null(dataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //The dataStore reads the changes correctly
            Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
        }

```
> Note: Read Queries performed during a session will be take into account any uncommitted operations in that session.
> So the resultset will include any changes already requested (but not yet committed).

```
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

            //We have a queued update operation
            Assert.NotNull(dataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //We have not execute any update operations
            Assert.Null(dataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));

            //The underlying database has NOT changed
            Assert.Equal("Toyota", inMemoryDb.OfType<Car>().Single(car => car.id == carId).Make);

            //The DataStore instance picks up the change, because it has applied
            //all the previous changes made during this session to any query.
            Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
        }
```

> Using a DataStore instance across several consecutive sessions (sets of changes followed by a call to CommitChanges()) 
> is perfectly acceptable. Just note, that if you query the EventAggregator.Events collection you will see the IDataStoreEvents
> from all sessions, but those already committed will be marked as Committed. The reason we do not remove events afer CommitChanges()
> is called is to allow you to query their performance metrics later on.

