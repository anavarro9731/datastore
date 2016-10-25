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
    public class DataStoreCreateCapabilitiesTests
    {
        [Fact]
        public async void WhenCallingTheCreateWithoutCommitting_ItShouldOnlyMakeTheChangesLocally()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingTheCreateWithoutCommitting_ItShouldOnlyMakeTheChangesLocally));

            var newCar = new Car()
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            var result = await testHarness.DataStore.Create(newCar);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateAdded<Car>));
            Assert.Equal(0, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
            Assert.True(result.Active); 
        }

        [Fact]
        public async void WhenCallingCommitAfterCreate_ItShouldPersistChangesToTheDatabase()
        {
            // Given
            var testHarness = GetTestHarness(nameof(WhenCallingCommitAfterCreate_ItShouldPersistChangesToTheDatabase));

            var newCar = new Car()
            {
                id = Guid.NewGuid(),
                Make = "Volvo"
            };

            //When
            await testHarness.DataStore.Create(newCar);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateAdded<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.True(testHarness.QueryDatabase<Car>().Result.Single().Active);
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
        }
    }
}
