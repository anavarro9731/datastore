namespace DataStore.Tests.Tests.IDocumentRepository.Repo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingAddAsyncDynamically
    {
        private Guid newCarId;

        private Car result;

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact(
            Skip = @"this test makes clear an issue where this.result doesn't include new 
        but uncommitted history this is an interesting issue which is not really a bug because 
        history is reloaded each operation but does leave the caller depending on their expectation
        with an outdated model, its a similar story for eTag")]
        public async void ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistory);
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingAddAsyncDynamically));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            //When
            await this.testHarness.DataStore.DocumentRepository.CreateAsync(newCar, "Test");

            this.versionHistory = this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.newCarId)).Single().VersionHistory;
        }
    }
}