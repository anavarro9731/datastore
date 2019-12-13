namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithTheReadOnlyFlagSetToTrue
    {
        private  Guid newCarId;

        private  ITestHarness testHarness;

        private async Task Setup()
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
            await this.testHarness.DataStore.Create(newCar, true);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().ReadOnly);
        }

        [Fact]
        public async void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            await Setup();
            Assert.True((await this.testHarness.DataStore.ReadActiveById<Car>(this.newCarId)).ReadOnly);
        }
    }
}