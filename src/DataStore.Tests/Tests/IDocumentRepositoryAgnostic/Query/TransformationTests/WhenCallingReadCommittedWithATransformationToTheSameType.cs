namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.TransformationTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedWithATransformationToTheSameType
    {
        private readonly Car carFromDatabase;

        private readonly ITestHarness testHarness;

        public WhenCallingReadCommittedWithATransformationToTheSameType()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedWithATransformationToTheSameType));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            // When
            this.carFromDatabase = this.testHarness.DataStore.Advanced.ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId)).Result.Single();
        }

        [Fact]
        public void YouCanReturnResultsOfTheSameTypeAsTheOneYouQueried()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.Equal(typeof(Car), this.carFromDatabase.GetType());
        }
    }
}