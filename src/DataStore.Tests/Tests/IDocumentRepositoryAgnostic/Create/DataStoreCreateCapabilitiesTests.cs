//using System;
//using System.Linq;
//using DataStore.Models.Messages;
//using DataStore.Tests.Constants;
//using DataStore.Tests.Models;
//using DataStore.Tests.TestHarness;
//using Xunit;

//namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic
//{
//    [Collection(TestCollections.RunSerially)]
//    public class DataStoreCreateCapabilitiesTests
//    {
//        [Fact]
//        public async void WhenCallingCreateWithoutCommitting_ItShouldOnlyMakeTheChangesInSession()
//        {
//            // Given
//            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateWithoutCommitting_ItShouldOnlyMakeTheChangesInSession));

//            var newCar = new Car()
//            {
//                id = Guid.NewGuid(),
//                Make = "Volvo"
//            };

//            //When
//            var result = await testHarness.DataStore.Create(newCar);

//            //Then
//            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
//            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is CreateOperation<Car>));
//            Assert.Equal(0, testHarness.QueryDatabase<Car>().Result.Count());
//            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
//            Assert.True(result.Active); 
//        }

//        [Fact]
//        public async void WhenCallingCommitAfterCreate_ItShouldPersistChangesToTheDatabase()
//        {
//            // Given
//            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterCreate_ItShouldPersistChangesToTheDatabase));

//            var newCarId = Guid.NewGuid();
//            var newCar = new Car()
//            {
//                id = newCarId,
//                Make = "Volvo"
//            };

//            //When
//            await testHarness.DataStore.Create(newCar);
//            await testHarness.DataStore.CommitChanges();

//            //Then
//            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is CreateOperation<Car>));
//            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
//            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
//            Assert.True(testHarness.QueryDatabase<Car>().Result.Single().Active);
//            Assert.True(testHarness.QueryDatabase<Car>().Result.Single().id == newCarId);
//            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
//        }

//        [Fact]
//        public async void WhenCallingCommitAfterCreate_ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
//        {
//            // Given
//            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCommitAfterCreate_ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned));

//            var newCarId = Guid.NewGuid();
//            var newCar = new Car()
//            {
//                id = newCarId,
//                Make = "Volvo"
//            };

//            var result = await testHarness.DataStore.Create(newCar);
//            //When
//            result.id = Guid.NewGuid();
//            await testHarness.DataStore.CommitChanges();

//            //Then
//            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is CreateOperation<Car>));
//            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
//            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
//            Assert.True(testHarness.QueryDatabase<Car>().Result.Single().Active);
//            Assert.True(testHarness.QueryDatabase<Car>().Result.Single().id == newCarId);
//            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
//        }
//    }
//}
