namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.TransformationTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.PureFunctions.Extensions;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedWithATransformation
    {
        public WhenCallingReadCommittedWithATransformation()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedWithATransformation));

            carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            // When
            testHarness.DataStore.Advanced.ReadCommitted(
                    (IQueryable<Car> cars) => cars.Where(car => car.id == carId)
                        .Select(c =>
                            new {c.id, c.Make})
                )
                .Result.Single()
                .Op(result =>
                {
                    transformedType.Id = result.id;
                    transformedType.Make = result.Make;
                });
        }

        private (Guid Id, string Make) transformedType;
        private readonly Guid carId;

        [Fact]
        public void ItShouldReturnTheTransformedTypeWithTheRightValues()
        {
            Assert.Equal("Volvo", transformedType.Make);
            Assert.Equal(carId, transformedType.Id);
        }
    }
}