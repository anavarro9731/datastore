namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession
    {
        private readonly Car carFromDatabase;

        private readonly ITestHarness testHarness;

        public WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(existingCar);

            this.testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            // When
            this.carFromDatabase = this.testHarness.DataStore.DirectToDb.Read<Car>(car => car.id == carId).Result.Single();
        }

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.NotNull(this.carFromDatabase);
        }
    }
}