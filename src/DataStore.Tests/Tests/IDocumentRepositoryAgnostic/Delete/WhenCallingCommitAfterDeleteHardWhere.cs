namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingCommitAfterDeleteHardWhere
    {
        public WhenCallingCommitAfterDeleteHardWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingCommitAfterDeleteHardWhere));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly IEnumerable<Car> result;
        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }


        [Fact]
        public void ItShouldReturnTheItemsDeleted()
        {
            Assert.Equal(carId, result.Single().id);
        }
    }
}