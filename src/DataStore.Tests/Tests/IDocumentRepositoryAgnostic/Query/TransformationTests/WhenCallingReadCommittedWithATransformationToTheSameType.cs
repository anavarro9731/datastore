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
        public WhenCallingReadCommittedWithATransformationToTheSameType()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(YouCanReturnResultsOfTheSameTypeAsTheOneYouQueried));

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
            Assert.NotNull(testHarness.Operations.SingleOrDefault(e => e is TransformationQueriedOperation<Car>));
            Assert.Equal(carFromDatabase.GetType(), typeof(Car));
        }
    }
}