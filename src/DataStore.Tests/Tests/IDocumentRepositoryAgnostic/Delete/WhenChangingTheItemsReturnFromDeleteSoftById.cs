namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Interfaces.Events;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnFromDeleteSoftById
    {
        public WhenChangingTheItemsReturnFromDeleteSoftById()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnFromDeleteSoftById
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
            //Then

            Assert.NotNull(Enumerable.SingleOrDefault<IDataStoreOperation>(testHarness.Operations, e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(Enumerable.SingleOrDefault<IQueuedDataStoreWriteOperation>(testHarness.QueuedWriteOperations, e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(Enumerable.Single<Car>(testHarness.QueryDatabase<Car>(cars => Queryable.Where<Car>(cars, car => car.id == carId))).Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}