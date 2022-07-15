namespace DataStore.Tests.Tests.Create
{
    #region

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

    #endregion

    public class WhenCallingCreate
    {
        private Guid newCarId;

        private Car result;

        private ITestHarness testHarness;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldCreateAVersionHistoryRecordInTheAggregate()
        {
            await Setup();
            Assert.Single(this.versionHistory);
        }
        
        [Fact]
        public async void ItShouldFlushTheQueue()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is CreateOperation<Car> && e.MethodCalled == nameof(DataStore.Create)));
            Assert.True(this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().Active);
            Assert.True(this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().id == this.newCarId);
        }

        [Fact]
        public async void ItShouldReflectTheChangeInAQueryFromTheSameSession()
        {
            await Setup();
            Assert.Single(await this.testHarness.DataStore.ReadActive<Car>());
        }

        [Fact]
        public async void ItShouldSetTheTimeStamps()
        {
            await Setup();
            Assert.NotEqual(default, this.result.Created);
            Assert.NotEqual(default, this.result.Modified);
            Assert.NotEqual(default, this.result.CreatedAsMillisecondsEpochTime);
            Assert.NotEqual(default, this.result.ModifiedAsMillisecondsEpochTime);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreate));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            //When
            this.result = await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();

            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.newCarId))
                                      .Single().VersionHistory;
        }
    }
}