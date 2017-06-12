using System;
using System.Linq;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    public class WhenCallingCreateWithTheReadOnlyFlagSetToTrue
    {
        public WhenCallingCreateWithTheReadOnlyFlagSetToTrue()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateWithTheReadOnlyFlagSetToTrue));

            newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            //When
            testHarness.DataStore.Create(newCar, true).Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.True(testHarness.QueryDatabase<Car>().Single().ReadOnly);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Assert.True(testHarness.DataStore.ReadActiveById<Car>(newCarId).Result.ReadOnly);
        }
    }
}