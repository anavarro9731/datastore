namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardById
    {
        private Guid carId;

        private Car result;

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistoryBeforeHardDelete;

        [Fact]
        public async void ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistoryBeforeHardDelete);
        }

        [Fact]
        public async void ItShouldFlushTheSessionCache()
        {
            await Setup();
            Assert.Empty(this.testHarness.DataStore.QueuedOperations);
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is HardDeleteOperation<Car> && e.MethodCalled == nameof(DataStore.DeleteById)));
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
        }

        [Fact]
        public async void ItShouldReturnTheItemDeleted()
        {
            await Setup();
            Assert.Equal(this.carId, this.result.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardById));

            this.carId = Guid.NewGuid();
            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();

            this.versionHistoryBeforeHardDelete = this.testHarness
                                                      .QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId))
                                                      .Single().VersionHistory;

            //When
            this.result = await this.testHarness.DataStore.DeleteById<Car>(this.carId, o => o.Permanently());
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}