namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardWhere
    {
        private readonly ITestHarness testHarness;

        public WhenChangingTheItemsReturnedFromDeleteHardWhere()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteHardWhere));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    Id = carId,
                    Make = "Volvo"
                });

            var result = this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.Id == carId).Result;

            //When
            result.Single().Id = Guid.NewGuid(); //change in memory before commit
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}