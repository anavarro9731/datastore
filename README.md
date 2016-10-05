# DataStore

A Document-Centric Data Access Framework for Azure DocumentDB

##Overview

DataStore is an easy-to-use, data-access framework, which maps POCO C# classes to documents.

It supports basic CRUD operations on any C# object, with some additional features such as:

* Strongly typed mapping between documents and C# class types with generics
* Support for LINQ queries against objects and their children (where the DocumentDB client supports it)
* In-memory database, and event history for testing
* Id and timestamp management of object hierarchies
* Automatic retries of queries when limits are exceeded

DataStore is built with .NET Core SDK v.1.0.0-preview2-003131 tools but requires TFM net451. 

This is mainly because the DocumentDB Client Library does not support .NET Core yet.

##Roadmap

* Better documentation of API features
* Tracing and profiling
* Limited cross-document transactional support
* Partition support 
* Workflows (i.e. long running transaction support)
* Document-level Security

##Usage

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

`d.SoftDeleteById<Car>(car.Id);`

Find It

`var toyotaCars = d.Read<Car>(query => query.Where(c => c.Model = "Toyota"));`

or

`var myToyota = d.ReadActiveById<Car>(car.Id);`

See IDataStore.cs for the full list of supported methods.

###Unit Test Example

using Xunit;
...

   [Fact]
    public async void CanUpdateUser()
    {
        var documentRepository = new InMemoryDocumentRepository();
        var inMemoryDb = documentRepository.Aggregates;
        var eventAggregator = new EventAggregator { PropogateDomainEvents = false, PropogateDataStoreEvents = true, AddQueryEventsToEvents = false };
        var dataStore = new DataStore(documentRepository, EventAggregator);

        var userId = Guid.NewGuid();
        
        //Given
        inMemoryDb.Add(new User() {
            id = userId,
            Email = "roguetrader@therebellion.org"
        });

        //When
        await dataStore.UpdateById<User>(userId, user => user.Email = "redarteugor@therebellion.org");

        //Then the user is updated
        var userUpdated = eventAggregator.Events.SingleOrDefault(e => e is AggregateUpdated<User>);
        Assert.NotNull(userUpdated);
        Assert.Equal(userUpdated.As<AggregateUpdated<User>>().Model.Email, "redarteugor@therebellion.org");
	}

