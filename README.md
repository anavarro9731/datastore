# DataStore

A Data Access Framework, with a Azure CosmosDb implementation that expands on the features of the native .NET SDK

[Change Log](ChangeLog.md)

Support for other databases is possible if you are willing to write a provider implmenting the `IDocumentStore` interface.

## Overview

DataStore is an easy-to-use, data-access framework for C# which extends the basic LINQ query
capabilities on C# POCO objects with many additional features including:

* Paging with continuation tokens
* Increased consistency with session-based unit of work 
* In-memory database, and event history for testing (e.g. DataStore.ExecutedOperations.Where(o => o...)
* Optimistic Concurrency supported by default with the ability to disable on a per-query basis
* Profiling (e.g. Duration and Query Cost in Request Units)
* Automatic ID and timestamp management of object hierarchies 
* Automatic retries of queries when limits are exceeded
* Session-based vs Direct-to-Database views of data. 
Using a feature called "Event Replay" queries by default will reflect all previous changes made in the session. 
This is can be circumvented by choosing to use the "WithoutEventReplay" option on any call.
Some features such as Continue/Take, and OrderBy/ThenBy are available only WithoutEventReplay for obvious reasons.
* Document-level security (see the WhenCallingReadWithAuthorisation test in Datastore.Tests/IDocumentRepository/Query/)

**[See extensive test suite for examples of these and other features]**

DataStore targets both the NetStandard2.0 and .NET Framework 4.6.1 platforms and does not require .NET Core.

## Roadmap
* Better documentation of API features
* Update Version History on instances in the current session post-commit
* Add Performance Tracing
* Partitioned Collection Support 
  Currently on a single partition is supported. Azure does allow partitions to grow
  without restriction now, and manages this for you, but some may prefer partitioning on Id or ClassName)
* MartenDB implementation

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
Create a new `DataStore` object in the following 3 steps:
```
var s = new CosmosSettings(
				authorizationKey,
                containerName, 
				databaseName, 
				endpointUrl
			);

var r = s.CreateRespository();
// The respository is an expensive item to initialise (1-2 seconds) and for all practical purposes stateless so you should probably have only one per process.

var d = new DataStore.DataStore(r);
// The DataStore class is cheap to create, and it stores a record of both commmitted and uncommitted changes in its internal state.
// The DataStore.CommitChanges() method can be called multiple times on the same instance without harm. 
// This class is most suitable for use in handling a single message or API call, etc.

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

#### Sessions

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
> from all sessions, but those already committed will be marked as Committed. The reason we do not remove events after CommitChanges()
> is called is to allow you to query their performance metrics later on.

#### Options 

There are two levels of settings one at the store level and one at query level.

##### Store-level 

```
            var dataStoreOptions = DataStoreOptions.Create()
                .WithSecurity(scopeHierarchy)
                .EnableFullVersionHistory()
                .DisableOptimisticConcurrency();
            
            new DataStore(
                    new CosmosDbRepository(
                    cosmosStoreSettings),
                    dataStoreOptions: dataStoreOptions))
```

##### Query-level 

These options are specific as an optional lambda function on most Datastore methods.
They make possible a number of things including:

Skip/Take: (see WhenCallingReadActiveWithSkipAndTake.cs)
```
this.carsFromDatabase = await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car>(
                                        car => car.Make == "Volvo",
                                        o => o.ContinueFrom(firstContinuationToken).Take(2, ref secondContinuationToken));
```

Order By: (see WhenCallingReadWithOrderBy.cs)           
```
this.carsFromDatabaseOrderedAscending =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(car => car.Make == "Volvo", o => o.OrderBy(c => c.id));
```

Security: (see WhenCallingReadWithAuthorisation.cs)
```
await this.testHarness.DataStore.DeleteWhere<Car>(car => car.Make == "Volvo", o => o.AuthoriseFor(this.user));
```

and others. The lambda delegate sigature makes the options for a given query discoverable.
