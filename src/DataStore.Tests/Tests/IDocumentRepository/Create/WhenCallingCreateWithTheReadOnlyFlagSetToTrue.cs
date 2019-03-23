namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithTheReadOnlyFlagSetToTrue
    {
        private  Guid newCarId;

        private  ITestHarness testHarness;

        void Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithTheReadOnlyFlagSetToTrue));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            //When
            this.testHarness.DataStore.Create(newCar, true).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Setup();
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().ReadOnly);
        }

        [Fact]
        public void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            Setup();
            Assert.True(this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId).Result.ReadOnly);
        }
    }
}