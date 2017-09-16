namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardById
    {
        public WhenCallingDeleteHardById()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardById));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            result = testHarness.DataStore.DeleteHardById<Car>(carId).Result;
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Car result;
        private readonly Guid carId;

        [Fact]
        public async void ItShouldFlushTheSessionCache()
        {
            Assert.Empty(testHarness.DataStore.QueuedOperations);
            Assert.Empty(await testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public void ItShouldReturnTheItemDeleted()
        {
            Assert.Equal(carId, result.id);
        }
    }
}