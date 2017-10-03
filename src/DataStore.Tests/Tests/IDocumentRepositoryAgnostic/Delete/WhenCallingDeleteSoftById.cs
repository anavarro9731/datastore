namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftById
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteSoftById()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteSoftById));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            //When
            this.testHarness.DataStore.DeleteSoftById<Car>(this.carId).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}