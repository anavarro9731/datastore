namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Interfaces.Events;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere
    {
        public WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });


            var result = testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId).Result;

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly Guid carId;
        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}