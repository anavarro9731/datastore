namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.TransformationTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadCommittedWithATransformation
    {
        private readonly Guid carId;

        private (Guid Id, string Make) transformedType;

        public WhenCallingReadCommittedWithATransformation()
        {
            // Given
            var testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadCommittedWithATransformation));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            testHarness.DataStore.DeleteHardById<Car>(this.carId).Wait();

            // When
            testHarness.DataStore.Advanced.ReadCommitted(
                (IQueryable<Car> cars) => cars.Where(car => car.id == this.carId).Select(
                    c => new
                    {
                        c.id,
                        c.Make
                    })).Result.Single().Op(
                result =>
                    {
                    this.transformedType.Id = result.id;
                    this.transformedType.Make = result.Make;
                    });
        }

        [Fact]
        public void ItShouldReturnTheTransformedTypeWithTheRightValues()
        {
            Assert.Equal("Volvo", this.transformedType.Make);
            Assert.Equal(this.carId, this.transformedType.Id);
        }
    }
}