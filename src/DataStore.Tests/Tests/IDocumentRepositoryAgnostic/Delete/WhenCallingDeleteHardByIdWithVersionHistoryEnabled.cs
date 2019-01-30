namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardByIdWithVersionHistoryEnabled
    {
        private readonly Guid carId;

        private readonly Car result;

        private readonly ITestHarness testHarness;

        private readonly Guid unitOfWorkId;

        public WhenCallingDeleteHardByIdWithVersionHistoryEnabled()
        {
            // Given
            this.carId = Guid.NewGuid();

            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingDeleteHardByIdWithVersionHistoryEnabled),
                new DataStoreOptions
                {
                    UnitOfWorkId = this.unitOfWorkId,
                    UseVersionHistory = true
                });

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                }).Wait();

            this.testHarness.DataStore.CommitChanges().Wait();
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.NotEmpty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());

            //When
            this.result = this.testHarness.DataStore.DeleteHardById<Car>(this.carId).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldDeleteAllTheHistory()
        {
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistory<Car>>());
            Assert.Empty(this.testHarness.QueryDatabase<AggregateHistoryItem<Car>>());
        }
        
    }
}