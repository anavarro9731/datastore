namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCommittingMultipleBatchesOfChanges
    {
        private Guid carId = Guid.NewGuid();

        private ITestHarness testHarness;
            
        private List<Aggregate.AggregateVersionInfo> versionHistory;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCommittingMultipleBatchesOfChanges));

            var newCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToSecondsEpochTime()
            };

            var result = await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();

            result.Make = "Ford";
            var result2  = await this.testHarness.DataStore.Update(result);
            await this.testHarness.DataStore.CommitChanges();

            result2.Make = "Alfa Romeo";
            var result3 = await this.testHarness.DataStore.Update(result2);
            await this.testHarness.DataStore.CommitChanges();

            //When
            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => 
                cars.Where(c => c.id == this.carId)).Single().VersionHistory;
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
    }
}