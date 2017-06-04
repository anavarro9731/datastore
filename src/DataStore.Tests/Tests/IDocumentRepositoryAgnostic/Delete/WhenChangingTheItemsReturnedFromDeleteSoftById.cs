namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteSoftById
    {
        public WhenChangingTheItemsReturnedFromDeleteSoftById()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnedFromDeleteSoftById
                ));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });


            var result = testHarness.DataStore.DeleteSoftById<Car>(carId).Result;

            //When
            result.id = Guid.NewGuid(); //change in memory before commit
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}