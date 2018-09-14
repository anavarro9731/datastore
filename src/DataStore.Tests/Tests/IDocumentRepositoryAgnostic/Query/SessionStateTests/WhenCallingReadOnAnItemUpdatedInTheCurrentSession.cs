namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadOnAnItemUpdatedInTheCurrentSession
    {
        private readonly Car carFromSession;

        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingReadOnAnItemUpdatedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadOnAnItemUpdatedInTheCurrentSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford").Wait();

            // When
            this.carFromSession = this.testHarness.DataStore.Read<Car>(car => car.Make == "Ford").Result.Single();
        }

        [Fact]
        public void ItShouldReturnTheItemWithTheUpdatesAppliedWhenThePredicateMatches()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Equal("Volvo", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.carFromSession.Make);
            Assert.Equal(this.carId, this.carFromSession.id);
        }
    }
}