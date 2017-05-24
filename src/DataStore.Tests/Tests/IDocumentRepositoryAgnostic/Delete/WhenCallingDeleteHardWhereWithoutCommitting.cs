namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Interfaces.Events;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereWithoutCommitting
    {
        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardWhereWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesInSession));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Wait();
        }

        [Fact]
        public async void WhenCallingDeleteHardWhereWithoutCommitting_ItShouldOnlyMakeChangesInSession()
        {
            //Then
            Assert.Null(Enumerable.SingleOrDefault<IDataStoreOperation>(testHarness.Operations, e => e is HardDeleteOperation<Car>));
            Assert.NotNull(Enumerable.SingleOrDefault<IQueuedDataStoreWriteOperation>(testHarness.QueuedWriteOperations, e => e is QueuedHardDeleteOperation<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}