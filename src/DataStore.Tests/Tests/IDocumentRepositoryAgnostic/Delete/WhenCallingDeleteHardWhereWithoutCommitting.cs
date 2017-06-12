namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereWithoutCommitting
    {
        public WhenCallingDeleteHardWhereWithoutCommitting()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteHardWhereWithoutCommitting));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Wait();
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldOnlyMakeChangesInSession()
        {
            Assert.Null(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.NotEmpty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}