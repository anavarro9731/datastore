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
        public async void CanCreateWithoutCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanCreateWithoutCommit));

            var newCar = new Car()
            {
                id = Guid.NewGuid(),
                Active = true,
                Make = "Volvo"
            };

            //When
            await testHarness.DataStore.Create(newCar);

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateAdded<Car>));
            Assert.Equal(0, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
        }

        [Fact]
        public async void CanCreateWithCommit()
        {
            // Given
            var testHarness = GetTestHarness(nameof(CanCreateWithCommit));

            var newCar = new Car()
            {
                id = Guid.NewGuid(),
                Active = true,
                Make = "Volvo"
            };

            //When
            await testHarness.DataStore.Create(newCar);
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.NotNull(testHarness.Events.SingleOrDefault(e => e is AggregateAdded<Car>));
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Result.Count());
            Assert.Equal(1, testHarness.DataStore.ReadActive<Car>(car => car).Result.Count());
        }
    }
}
