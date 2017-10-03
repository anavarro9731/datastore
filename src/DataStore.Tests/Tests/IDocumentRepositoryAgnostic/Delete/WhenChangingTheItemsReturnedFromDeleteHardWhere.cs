namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
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
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemsReturnedFromDeleteHardWhere));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            var result = this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Result;

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
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