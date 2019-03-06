namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithOrderBy
    {
        private readonly IEnumerable<Car> carsFromDatabaseOrderedAscending;

        private readonly IEnumerable<Car> carsFromDatabaseOrderedDescending;

        private readonly Guid secondCarId;

        private readonly ITestHarness testHarness;

        private readonly Guid thirdCarId;

        public WhenCallingReadWithOrderBy()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadWithOrderBy));

            var activeCarId = Guid.Parse("c74bbd8f-b9c4-4529-ba55-6b920c4b4a42");
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            this.secondCarId = Guid.Parse("ae9dea20-538c-44ab-b372-9bd2e7ddd1c8");
            var thirdExistingCar = new Car
            {
                id = this.secondCarId,
                Active = true,
                Make = "Volvo"
            };

            this.thirdCarId = Guid.Parse("fac65251-261a-4c6e-b13c-0d9d80e2b761");
            var fourthExistingCar = new Car
            {
                id = this.thirdCarId,
                Active = true,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(thirdExistingCar);
            this.testHarness.AddToDatabase(fourthExistingCar);

            // When
            this.carsFromDatabaseOrderedAscending =
                this.testHarness.DataStore.Read<Car, DefaultQueryOptions<Car>>(o => o.OrderBy(c => c.id), car => car.Make == "Volvo").Result;

            this.carsFromDatabaseOrderedDescending = this.testHarness.DataStore
                                                         .Read<Car, DefaultQueryOptions<Car>>(o => o.OrderBy(c => c.id, true), car => car.Make == "Volvo").Result;
        }

        [Fact]
        public void ItShouldReturnTheCarsInTheOppositeOrderOfWhenTheyAreOrderedDescending()
        {
            Assert.Equal(3, this.carsFromDatabaseOrderedAscending.Count());
            Assert.Equal(3, this.carsFromDatabaseOrderedDescending.Count());
            Assert.Equal(this.secondCarId, this.carsFromDatabaseOrderedDescending.Last().id);
            Assert.Equal(this.secondCarId, this.carsFromDatabaseOrderedAscending.First().id);
        }
    }
}