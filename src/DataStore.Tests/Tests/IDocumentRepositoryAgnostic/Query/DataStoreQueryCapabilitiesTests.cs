using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Newtonsoft.Json;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
{
    [Collection(TestCollections.RunSerially)]
    public class DataStoreQueryCapabilitiesTests
    {
        [Fact]
        public async void WhenCallingRead_ItShouldReturnAllItemsRegardlessOfActiveFlag()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingRead_ItShouldReturnAllItemsRegardlessOfActiveFlag));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(activeExistingCar);
            await testHarness.AddToDatabase(inactiveExistingCar);

            // When
            var carsFromDatabase = (await testHarness.DataStore.Read<Car>(cars => cars.Where(car => car.Make == "Volvo")));

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(2, carsFromDatabase.Count());
        }

        [Fact]
        public async void WhenCallingReadActive_ItShouldOnlyReturnActiveItems()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActive_ItShouldOnlyReturnActiveItems));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
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
            Assert.Equal(2, testHarness.Operations.Count(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal("Volvo", activeCarFromDatabase.Make);
            Assert.Null(inactiveCarFromDatabase);
        }

        [Fact]
        public async void WhenCallingExists_ItShouldReturnTheItemIfItExistsWhileConsideringChangesAlreadyMadeInTheSession()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExists_ItShouldReturnTheItemIfItExistsWhileConsideringChangesAlreadyMadeInTheSession));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(activeExistingCar);
            await testHarness.DataStore.DeleteHardById<Car>(activeCarId);

            // When
            var activeCarFromDataStore = (await testHarness.DataStore.ReadActive<Car>(cars => cars.Where(car => car.id == activeCarId))).SingleOrDefault();

            //Then
            Assert.Null(activeCarFromDataStore);
        }

        [Fact]
        public async void WhenCallingExists_ItShouldReturnTheItemIfItExistsRegardlessOfStatus()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExists_ItShouldReturnTheItemIfItExistsRegardlessOfStatus));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
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
            var activeCarExists = await testHarness.DataStore.Exists(activeCarId);
            var inactiveCarExists = await testHarness.DataStore.Exists(inactiveCarId);

            //Then
            Assert.True(inactiveCarExists);
            Assert.True(activeCarExists);
            
        }

        [Fact]
        public async void WhenCallingExists_ItShouldReturnFalseIfTheItemDoesNotExist()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingExists_ItShouldReturnFalseIfTheItemDoesNotExist));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(activeExistingCar);
            
            // When
            var carExists = await testHarness.DataStore.Exists(Guid.NewGuid());
            
            //Then
            Assert.False(carExists);
        }

        [Fact]
        public async void WhenCallingReadCommittedById_ItShouldReturnTheItemWithThatIdWithoutConsiderationForSessionChanges()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedById_ItShouldReturnTheItemWithThatIdWithoutConsiderationForSessionChanges));

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
            Car carFromDatabase;
            var document = await testHarness.DataStore.Advanced.ReadCommittedById<Car>(carId);
            try
            {
                carFromDatabase = (Car) (dynamic) document;
            }
            catch (Exception)
            {
                carFromDatabase = JsonConvert.DeserializeObject<Car>(JsonConvert.SerializeObject(document));
            }

            //Then
            Assert.NotNull(testHarness.Operations.Exists(e => e is AggregateQueriedByIdOperation));
            Assert.Equal("Volvo", carFromDatabase.Make);
            Assert.Equal(carId, carFromDatabase.id);
        }
        
        [Fact]
        public async void WhenCallingReadActiveById_ItShouldReturnTheItemWithThatIdOnlyIfItIsActive()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveById_ItShouldReturnTheItemWithThatIdOnlyIfItIsActive));

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
            var inactiveCarFromDatabase = await testHarness.DataStore.ReadActiveById<Car>(inactiveCarId);

            //Then
            Assert.Equal(2, testHarness.Operations.Count(e => e is AggregateQueriedByIdOperation));
            Assert.Equal(activeCarId, activeCarFromDatabase.id);
            Assert.Null(inactiveCarFromDatabase);
        }

        [Fact]
        public async void WhenQueryingItemsAfterOtherHaveBeenUpdatedInTheCurrentSession_ItShouldApplyThoseUpdatesToTheResultSet()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenQueryingItemsAfterOtherHaveBeenUpdatedInTheCurrentSession_ItShouldApplyThoseUpdatesToTheResultSet));

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
            Assert.NotNull(testHarness.Operations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal("Volvo", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Result.Single().Make);
            Assert.Equal("Ford", carFromDatabase.Make);
        }

        [Fact]
        public async void WhenQueryingItemsAfterOthersHaveBeenHardDeletedInTheCurrentSession_ItShouldApplyThoseDeletesToTheResultSet()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenQueryingItemsAfterOthersHaveBeenHardDeletedInTheCurrentSession_ItShouldApplyThoseDeletesToTheResultSet));

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
            Assert.NotNull(testHarness.Operations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Null(readCarFromDatabase);
        }

        [Fact]
        public async void WhenQueryingItemsAfterOthersHaveBeenSoftDeletedInTheCurrentSession_ItShouldApplyThoseDeletesToTheResultSet()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenQueryingItemsAfterOthersHaveBeenSoftDeletedInTheCurrentSession_ItShouldApplyThoseDeletesToTheResultSet));

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
            Assert.NotNull(testHarness.Operations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.NotNull(readCarFromDatabase);
        }

        [Fact]
        public async void WhenQueryingItemsAfterNewItemsHaveBeenAddedInTheCurrentSession_ItShouldAddThoseItemsToTheResultSet()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenQueryingItemsAfterNewItemsHaveBeenAddedInTheCurrentSession_ItShouldAddThoseItemsToTheResultSet));

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
            Assert.NotNull(testHarness.Operations.All(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Equal(2, readCarFromDatabase);
            Assert.Equal(2, readActiveCarFromDatabase);
        }

        [Fact]
        public async void WhenUsingReadCommitted_ItShouldReturnResultsWithoutConsiderationForChangesMadeInThisSession()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenUsingReadCommitted_ItShouldReturnResultsWithoutConsiderationForChangesMadeInThisSession));

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
            var carFromDatabase = (await testHarness.DataStore.Advanced.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId))).Single();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.Equal("Volvo", carFromDatabase.Make);
        }

        [Fact]
        public async void WhenUsingReadCommitted_YouCanReturnResultsOfATypeDifferentToTheOneYouQueriedByUsingATransformation()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenUsingReadCommitted_YouCanReturnResultsOfATypeDifferentToTheOneYouQueriedByUsingATransformation));

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
            var transformedCar = (await testHarness.DataStore.Advanced.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId).Select(c => new { id = c.id, c.Make }))).Single();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e.TypeName == transformedCar.GetType().FullName));
            Assert.Equal("Volvo", transformedCar.Make);
        }

        [Fact]
        public async void WhenUsingCanReadCommitted_YouCanReturnResultsOfTheSameTypeAsTheOneYouQueried()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenUsingCanReadCommitted_YouCanReturnResultsOfTheSameTypeAsTheOneYouQueried));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            await testHarness.AddToDatabase(existingCar);

            // When
            var carFromDatabase = (await testHarness.DataStore.Advanced.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId))).Single();

            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.Equal("Volvo", carFromDatabase.Make);
        }
    }
}