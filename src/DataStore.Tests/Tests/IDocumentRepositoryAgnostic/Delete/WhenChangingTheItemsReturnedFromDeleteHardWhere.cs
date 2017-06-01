namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardWhere
    {
        public WhenChangingTheItemsReturnedFromDeleteHardWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned
                ));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Result;

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}