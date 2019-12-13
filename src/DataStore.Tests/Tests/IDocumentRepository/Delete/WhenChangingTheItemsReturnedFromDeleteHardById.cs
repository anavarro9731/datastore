namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardById

    {
        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteHardById));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            var result = await this.testHarness.DataStore.DeleteHardById<Car>(carId);

            //When
            result.id = Guid.NewGuid(); //change in memory before commit
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}