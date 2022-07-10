namespace DataStore.Tests.Tests.Delete
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models;
    using global::DataStore.Models.Messages;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereWithFullVersionHistory
    {
        private Guid carId;

        private IEnumerable<Car> result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(
                this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(
                    e => e is HardDeleteOperation<Car> && e.MethodCalled == nameof(DataStore.DeleteWhere)));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public async void ItShouldRemoveAllHistoryItems()
        {
            await Setup();
            Assert.Empty(this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>());
        }

        [Fact]
        public async void ItShouldReturnTheItemsDeleted()
        {
            await Setup();
            Assert.Equal(this.carId, this.result.Single().id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingDeleteHardWhereWithFullVersionHistory),
                DataStoreOptions.Create().EnableFullVersionHistory());

            this.carId = Guid.NewGuid();
            await this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            await this.testHarness.DataStore.CommitChanges();
            Assert.NotEmpty(this.testHarness.QueryUnderlyingDbDirectly<AggregateHistoryItem<Car>>());

            //When
            this.result = await this.testHarness.DataStore.DeleteWhere<Car>(car => car.id == this.carId, o => o.Permanently());
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}