namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemReturnedFromCreate
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenChangingTheItemReturnedFromCreate()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemReturnedFromCreate));

            this.newCarId = Guid.NewGuid();

            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            var result = this.testHarness.DataStore.Create(newCar).Result;

            //change the id before committing, if not cloned this would cause the item to be created with a different id
            result.id = Guid.NewGuid();

            //When
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
            Assert.NotNull(this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId).Result);
        }
    }
}