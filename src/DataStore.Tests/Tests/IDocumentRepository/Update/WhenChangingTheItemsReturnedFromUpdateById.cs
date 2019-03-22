namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromUpdateById
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemsReturnedFromUpdateById()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromUpdateById));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            var result = this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford").Result;

            //When
            result.id = Guid.NewGuid();
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