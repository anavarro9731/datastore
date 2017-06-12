namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemPassedIntoCreate
    {
        public WhenChangingTheItemPassedIntoCreate()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemPassedIntoCreate));

            newCarId = Guid.NewGuid();

            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            testHarness.DataStore.Create(newCar).Wait();

            //change the id before committing, if not cloned this would cause the item to be created with a different id
            newCar.id = Guid.NewGuid();

            //When
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            Assert.True(testHarness.QueryDatabase<Car>().Single().id == newCarId);
            Assert.NotNull(testHarness.DataStore.ReadActiveById<Car>(newCarId).Result);
        }
    }
}