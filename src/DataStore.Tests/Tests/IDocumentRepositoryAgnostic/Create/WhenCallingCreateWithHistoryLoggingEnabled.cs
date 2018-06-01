namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithHistoryLoggingEnabled
    {
        private readonly Guid newCarId;

        private readonly Guid unitOfWorkId;
        private readonly ITestHarness testHarness;

        public WhenCallingCreateWithHistoryLoggingEnabled()
        {
            
            // Given
            this.unitOfWorkId = Guid.NewGuid();

            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreate),
                new DataStoreOptions() { UnitOfWorkId = this.unitOfWorkId, UseVersionHistory = true});

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId,
                Make = "Volvo"
            };

            //When
            this.testHarness.DataStore.Create(newCar).Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }
        

        [Fact]  
        public void ItShouldCreateAnAggregateHistoryRecord()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<AggregateHistory<Car>>));
            Assert.True(this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().Active);
            Assert.Equal(this.newCarId, this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateId);
            Assert.Equal(1,this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().Version);
            Assert.Equal(1,this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single().VersionId);
            Assert.Equal(this.unitOfWorkId,this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single().UnitWorkId);
            Assert.Equal(typeof(Car).AssemblyQualifiedName,this.testHarness.QueryDatabase<AggregateHistory<Car>>().Single().AggregateVersions.Single().AssemblyQualifiedTypeName);
        }


        [Fact]
        public void ItShouldCreateAnAggregateHistoryItemRecord()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().Active);
            Assert.True(this.testHarness.QueryDatabase<Car>().Single().id == this.newCarId);
        }
    }
}