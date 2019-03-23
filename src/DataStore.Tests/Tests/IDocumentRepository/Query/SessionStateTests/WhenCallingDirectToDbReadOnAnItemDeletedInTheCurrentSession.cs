namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
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
            this.testHarness = TestHarness.Create(nameof(WhenCallingDirectToDbReadOnAnItemDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                Id = carId,
                Active = false,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(existingCar);

            this.testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            // When
            this.carFromDatabase = this.testHarness.DataStore.WithoutEventReplay.Read<Car>(car => car.Id == carId).Result.Single();
        }

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.NotNull(this.carFromDatabase);
        }
    }
}