namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCommittingMultipleBatchesOfChangesWithFullHistory
    {
        private readonly Guid carId = Guid.NewGuid();

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldCreateTheHistoryRecords()
        {
            await Setup();
            Assert.True(this.versionHistory.All(x => x.AggegateHistoryItemId != null));
            var aggregateHistoryItems = this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>();
            Assert.True(this.versionHistory.All(v => aggregateHistoryItems.Count(i => i.id == v.AggegateHistoryItemId) == 1));
            Assert.True(aggregateHistoryItems.All(x => x.VersionHistory.Count == 0));
            Assert.True(aggregateHistoryItems.All(x => x.AggregateVersion != null));
            Assert.Equal(3, aggregateHistoryItems.Count);
        }

        [Fact]
        public async void ItShouldIncrementTheVersionHistoryBatchIds()
        {
            await Setup();
            Assert.Equal(1, this.versionHistory.Last().CommitBatch);
            Assert.Equal(2, this.versionHistory.Skip(1).First().CommitBatch);
            Assert.Equal(3, this.versionHistory.First().CommitBatch);
            Assert.Equal(3, this.versionHistory.Count);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCommittingMultipleBatchesOfChangesWithFullHistory),
                DataStoreOptions.Create().EnableFullVersionHistory());

            var newCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToMillisecondsEpochTime()
            };

            var result = await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();

            result.Make = "Ford";
            var result2 = await this.testHarness.DataStore.Update(result);
            await this.testHarness.DataStore.CommitChanges();

            result2.Make = "Alfa Romeo";
            var result3 = await this.testHarness.DataStore.Update(result2);
            await this.testHarness.DataStore.CommitChanges();

            //When
            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single()
                                      .VersionHistory;
        }
    }
}
