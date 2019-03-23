namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateById
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateById()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateById));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            //When
            this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford").Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void _ItShouldPersistTheChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}