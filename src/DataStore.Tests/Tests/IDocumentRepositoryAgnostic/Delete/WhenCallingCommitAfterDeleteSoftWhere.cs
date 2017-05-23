namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingCommitAfterDeleteSoftWhere
    {
        public WhenCallingCommitAfterDeleteSoftWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingCommitAfterDeleteSoftWhere_ItShouldPersistTheChangesToTheDatabase));

            carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            //When
            testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId).Wait();
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid carId;

        [Fact]
        public async void WhenCallingCommitAfterDeleteSoftWhere_ItShouldPersistTheChangesToTheDatabase()
        {
            //Then
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(testHarness.QueuedWriteOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == carId)).Single().Active);
            Assert.Empty(await testHarness.DataStore.ReadActive<Car>(car => car));
            Assert.NotEmpty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}