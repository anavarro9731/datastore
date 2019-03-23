namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithTheReadOnlyFlagSetToTrue
    {
        private readonly Guid newCarId;

        private readonly ITestHarness testHarness;

        public WhenCallingCreateWithTheReadOnlyFlagSetToTrue()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithTheReadOnlyFlagSetToTrue));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                Id = this.newCarId,
                Make = "Volvo"
            };

            //When
            this.testHarness.DataStore.Create(newCar, true).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().ReadOnly);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Assert.True(this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId).Result.ReadOnly);
        }
    }
}