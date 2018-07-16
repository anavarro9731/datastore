namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhere
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteSoftWhere()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteSoftWhere));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            //When
            this.testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == this.carId).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldPersistTheChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}