namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardById

    {
        private readonly ITestHarness testHarness;

        public WhenChangingTheItemsReturnedFromDeleteHardById()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteHardById));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    Id = carId,
                    Make = "Volvo"
                });

            var result = this.testHarness.DataStore.DeleteHardById<Car>(carId).Result;

            //When
            result.Id = Guid.NewGuid(); //change in memory before commit
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