namespace DataStore.Tests.Tests.IDocumentRepository.SessionState
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCommitMultipleTimesAfterACreateOperation
    {
        private Car car;

        private Guid newCarId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldPersistChangesToTheDatabaseOnlyOnce()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is CreateOperation<Car>));
            Assert.True(this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().Active);
            Assert.True(this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().id == this.newCarId);
        }

        [Fact]
        public async void ItShouldReturnTheNewCarFromTheDatabaseWithSomeUpdateProperties()
        {
            await Setup();
            Assert.NotNull(this.car);
            Assert.Equal(this.car.Schema, typeof(Car).FullName);
            Assert.False(this.car.ReadOnly);
            Assert.NotNull(this.car.ScopeReferences);
        }

        [Fact]
        public async void ThereShouldBeNoOperationsInTheQueue()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedCreateOperation<Car>));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCommitMultipleTimesAfterACreateOperation));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo"
            };

            this.car = await this.testHarness.DataStore.Create(newCar);

            //When
            await this.testHarness.DataStore.CommitChanges();
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}