namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.TransformationTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedWithATransformationToTheSameType
    {
        public WhenCallingReadCommittedWithATransformationToTheSameType()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedWithATransformationToTheSameType));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            // When
            carFromDatabase = testHarness.DataStore.Advanced
                .ReadCommitted((IQueryable<Car> cars) => cars.Where(car => car.id == carId))
                .Result.Single();
        }

        private readonly Car carFromDatabase;
        private readonly ITestHarness testHarness;

        [Fact]
        public void YouCanReturnResultsOfTheSameTypeAsTheOneYouQueried()
        {
            Assert.NotNull(testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.Equal(typeof(Car), carFromDatabase.GetType());
        }
    }
}