namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhereWithoutCommitting
    {
        public WhenCallingDeleteSoftWhereWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteSoftWhereWithoutCommitting));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId).Wait();
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldOnlyMakeTheChangesInSession()
        {
            //Then
            Assert.Null(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}