namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateMoreThanTheVersionHistoryLimit
    {
        private const int Iterations = 15;

        private const int VersionHistoryLimit = 10;

        private Guid carId;

        private Car existingCar;

        private ITestHarness testHarness;

        private Car udpatedCar;

        private readonly Guid unitOfWorkId = Guid.NewGuid();

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldCreateTheVersionHistoryRecordsInTheAggregate()
        {
            await Setup();
            Assert.Equal(VersionHistoryLimit, this.versionHistory.Count);
            Assert.Equal(1, this.versionHistory.First().CommitBatch);
            Assert.Equal(Iterations, this.versionHistory.First().ChangeCount);
            Assert.Equal(Iterations - VersionHistoryLimit + 1, this.versionHistory.Last().ChangeCount);
            Assert.Equal(1, this.versionHistory.Last().CommitBatch);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateMoreThanTheVersionHistoryLimit),
                DataStoreOptions.Create().SpecifyUnitOfWorkId(this.unitOfWorkId));

            this.carId = Guid.NewGuid();

            this.existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToMillisecondsEpochTime()
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(this.existingCar);
            for (var i = 0; i < Iterations; i++)
            {
                var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);
                existingCarFromDb.Make = $"Ford{i}";

                //When
                this.udpatedCar = await this.testHarness.DataStore.Update(existingCarFromDb);
                await this.testHarness.DataStore.CommitChanges();
                this.testHarness.DataStore.MessageAggregator.Clear(); //-fake reset to clear commitbatch
            }

            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single()
                                      .VersionHistory;
        }
    }
}
