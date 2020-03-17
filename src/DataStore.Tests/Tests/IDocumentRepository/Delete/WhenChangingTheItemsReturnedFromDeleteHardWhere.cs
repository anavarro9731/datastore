namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardWhere
    {
        private ITestHarness testHarness;

         async  Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteHardWhere));

            var carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            var result = await this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId);

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}