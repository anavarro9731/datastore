namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Interfaces.Events;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnFromDeleteSoftDeleteWhere
    {
        public WhenChangingTheItemsReturnFromDeleteSoftDeleteWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnFromDeleteSoftDeleteWhere));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });


            var result = testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId).Result;

            //When
            Enumerable.Single<Car>(result).id = Guid.NewGuid(); //change in memory before commit
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly Guid carId;
        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.NotNull(Enumerable.SingleOrDefault<IDataStoreOperation>(testHarness.Operations, e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(Enumerable.SingleOrDefault<IQueuedDataStoreWriteOperation>(testHarness.QueuedWriteOperations, e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(Enumerable.Single<Car>(testHarness.QueryDatabase<Car>(cars => Queryable.Where<Car>(cars, car => car.id == carId))).Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}