namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromUpdateWhere
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemsReturnedFromUpdateWhere()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromUpdateWhere));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            var results = this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford").Result;

            //When
            foreach (var car in results) car.id = Guid.NewGuid();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}