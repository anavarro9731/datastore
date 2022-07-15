namespace DataStore.Tests.Tests.Delete
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingDeleteHardWhereWithoutCommitting
    {
        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldOnlyMakeChangesInSession()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryUnderlyingDbDirectly<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhereWithoutCommitting));

            var carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = carId, Make = "Volvo"
                });

            //When
            await this.testHarness.DataStore.DeleteWhere<Car>(car => car.id == carId, o => o.Permanently());
        }
    }
}