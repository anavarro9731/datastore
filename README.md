# DataStore

A Document-Centric Data Access Framework for Azure DocumentDB

## Overview

DataStore is an easy-to-use, data-access framework, which maps POCO C# classes to documents.

It supports basic CRUD operations on any C# object, with some additional features such as:

* Strongly typed mapping between documents and C# class types with generics
* Support for LINQ queries against objects and their children (where the DocumentDB client supports it)
* Limited cross-document transactional support (see transactions examples below)
* In-memory database, and event history for testing (see transactions examples below)
* Profiling (e.g. Duration and Query Cost in Request Units)
* Automatic Id and timestamp management of object hierarchies 
* Automatic retries of queries when limits are exceeded

DataStore is built with .NET Core SDK v.1.0.0-preview2-003131 tools but requires TFM net451. 

This is mainly because the DocumentDB Client Library does not support .NET Core yet.

## Roadmap

* Better documentation of API features
* Partitioned Collection support 
* Workflows (i.e. long running transaction support)
* Document-level Security

## Usage

Import the Nuget Package "DataStore".

Create a C# class which inherits DataStore.DataAccess.Models.Aggregate.
```
class Car : Aggregate {
	public string Make { get; set; }
	public string Model { get; set; }
}
```
Create a new DataStore object.
```
var d = new DataStore(new DocumentRepository(new DocumentDbSettings(
            string authorizationKey, 
            string databaseName, 
            string defaultCollectionName, 
            string endpointUrl)
			));
```
Save it to the database.

`var car = d.Create(new Car() { Make = "Toyota", Model = "Corolla"});`

Update it 

`d.UpdateById<Car>(car.id, (car) => car.Model = "Celica");`

or
```
car.Model = "Celica";
d.Update(car);
```

Delete It

`d.DeleteSoftById<Car>(car.Id);`

Find It

`var toyotaCars = d.Read<Car>(query => query.Where(c => c.Model = "Toyota"));`

or

`var myToyota = d.ReadActiveById<Car>(car.Id);`

See IDataStore.cs for the full list of supported methods.

### Transactions

Pending changes to the database are not committed by default, 
they are queued in the order received and stored as events in the EventAggregator.

However, Read Queries performed during a session will be intelligently merged with any uncommitted events in the session 
so the resultset will include any changes already requested (but not yet committed).

Calling DataStore.CommitChanges() will persist pending events to the database, and mark them as Committed. 

Using a DataStore instance across several consecutive sessions (sets of changes followed by a call to CommitChanges()) 
is perfectly acceptable. Just note, that if you query the EventAggregator.Events collection you will see the IDataStoreEvents
from all sessions, but those already committed will be marked as Committed. The reason we do not remove events afer CommitChanges()
is called is to allow you to query their performance metrics later on.


See the following XUnit examples for how this is used.

```    
public async void DoesNotUpdateCarInDatabase()
{
    var documentRepository = new InMemoryDocumentRepository();
    var inMemoryDb = documentRepository.Aggregates;
    var eventAggregator = new EventAggregator { PropogateDomainEvents = false, PropogateDataStoreEvents = true };
    var dataStore = new DataStore(documentRepository, eventAggregator);

    var carId = Guid.NewGuid();
        
    //Given
    inMemoryDb.Add(new Car() {
        id = userId,
        Make = "Toyota"
    });

    //When
    await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");

    //Then 
        
    //We have a AggregateUpdated event
    Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
        
    //The underlying database has not changed
    Assert.Equal("Toyota", inMemoryDb.OfType<Car>().Single(car => car.id == Guid.NewGuid()).Make);
        
    //The dataStore has applied pending changes to its collection
    Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
}

public async void CanUpdateCarInDatabase()
{
    var documentRepository = new InMemoryDocumentRepository();
    var inMemoryDb = documentRepository.Aggregates;
    var eventAggregator = new EventAggregator { PropogateDomainEvents = false, PropogateDataStoreEvents = true };
    var dataStore = new DataStore(documentRepository, eventAggregator);

    var carId = Guid.NewGuid();
        
    //Given
    inMemoryDb.Add(new Car() {
        id = userId,
        Make = "Toyota"
    });

    //When
    await dataStore.UpdateById<Car>(carId, car => car.Make = "Ford");
    await dataStore.CommitChanges();

    //Then 
        
    //We have a AggregateUpdated event
    Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateUpdated<Car>));
        
    //The underlying database has not changed
    Assert.Equal("Ford", inMemoryDb.OfType<Car>().Single(car => car.id == Guid.NewGuid()).Make);
        
    //The dataStore has applied pending changes to its collection
    Assert.Equal("Ford", dataStore.ReadActiveById<Car>(carId).Result.Make);
}
```
