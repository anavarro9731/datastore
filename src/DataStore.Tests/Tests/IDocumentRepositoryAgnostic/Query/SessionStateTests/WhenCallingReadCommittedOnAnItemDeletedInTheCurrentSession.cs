namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession
    {
        private readonly Car carFromDatabase;

        private readonly ITestHarness testHarness;

        public WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedOnAnItemDeletedInTheCurrentSession));

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
            this.carFromDatabase = this.testHarness.DataStore.Advanced.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId)).Result.Single();
        }

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.NotNull(this.carFromDatabase);
        }
    }
}