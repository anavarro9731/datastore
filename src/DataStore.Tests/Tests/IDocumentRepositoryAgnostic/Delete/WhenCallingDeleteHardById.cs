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
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteHardById));

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
        public async void ItShouldPersistChangesToTheDatabase()
        {
            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }

        [Fact]
        public void ItShouldReturnTheItemDeleted()
        {
            Assert.Equal(carId, result.id);
        }
    }
}