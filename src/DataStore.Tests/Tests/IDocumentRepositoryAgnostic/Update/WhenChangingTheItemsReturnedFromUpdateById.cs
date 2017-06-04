using System;
using System.Linq;
using DataStore.Models.Messages;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    public class WhenChangingTheItemsReturnedFromUpdateById
    {
        public WhenChangingTheItemsReturnedFromUpdateById()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnedFromUpdateById));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            result = testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Ford").Result;

            //When
            result.id = Guid.NewGuid();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Car result;
        private readonly Guid carId;

        [Fact]
        public void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            Assert.Equal("Ford", testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Make);
            Assert.Equal("Ford", testHarness.DataStore.ReadActiveById<Car>(carId).Result.Make);
        }
    }
}