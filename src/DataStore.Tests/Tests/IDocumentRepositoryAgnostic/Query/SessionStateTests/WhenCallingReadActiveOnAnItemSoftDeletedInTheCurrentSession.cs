namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession
    {
        private readonly Car carFromSession;

        private readonly ITestHarness testHarness;

        public WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            this.testHarness.DataStore.DeleteSoftById<Car>(carId).Wait();

            // When
            this.carFromSession = this.testHarness.DataStore.ReadActive<Car>(car => car.id == carId).Result.SingleOrDefault();
        }

        [Fact]
        public void ItShouldNotReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Single(this.testHarness.QueryDatabase<Car>());
            Assert.Null(this.carFromSession);
        }
    }
}