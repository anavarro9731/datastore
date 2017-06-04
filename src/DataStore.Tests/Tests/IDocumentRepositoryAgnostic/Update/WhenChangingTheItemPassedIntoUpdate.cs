namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemPassedIntoUpdate
    {
        public WhenChangingTheItemPassedIntoUpdate()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemPassedIntoUpdate));

            carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            //read from db to pickup changes to properties made by datastore oncreate
            var existingCarFromDb = testHarness.DataStore.ReadActiveById<Car>(carId).Result;
            existingCarFromDb.Make = "Ford";

            testHarness.DataStore.Update(existingCarFromDb).Wait();

            //change the id before committing, if not cloned this would cause the item not to be found
            existingCarFromDb.id = Guid.NewGuid();

            //When
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