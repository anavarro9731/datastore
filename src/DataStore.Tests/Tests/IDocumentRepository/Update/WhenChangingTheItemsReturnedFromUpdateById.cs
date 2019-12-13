namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromUpdateById
    {
        private Guid carId;

        private ITestHarness testHarness;

        async Task Setup()
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

            var result = await this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Make = "Ford");

            //When
            result.id = Guid.NewGuid();
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }
    }
}