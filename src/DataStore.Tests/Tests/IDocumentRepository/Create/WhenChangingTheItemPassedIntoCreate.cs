namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemPassedIntoCreate
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemPassedIntoCreate()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemPassedIntoCreate));

            this.newCarId = Guid.NewGuid();

            var newCar = new Car
            {
                Id = this.newCarId,
                Make = "Volvo"
            };

            this.testHarness.DataStore.Create(newCar).Wait();

            //change the Id before committing, if not cloned this would cause the item to be created with a different Id
            newCar.Id = Guid.NewGuid();

            //When
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().Id == this.newCarId);
            Assert.NotNull(this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId).Result);
        }
    }
}