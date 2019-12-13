namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemPassedIntoCreate
    {
        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
            Assert.NotNull(await this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId));
        }

         async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemPassedIntoCreate));

            this.newCarId = Guid.NewGuid();

            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            await this.testHarness.DataStore.Create(newCar);

            //change the id before committing, if not cloned this would cause the item to be created with a different id
            newCar.id = Guid.NewGuid();

            //When
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}