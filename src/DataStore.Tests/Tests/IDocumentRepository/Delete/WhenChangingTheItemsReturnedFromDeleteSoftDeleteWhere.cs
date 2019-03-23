namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteSoftDeleteWhere));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            var result = this.testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == this.carId).Result;

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}