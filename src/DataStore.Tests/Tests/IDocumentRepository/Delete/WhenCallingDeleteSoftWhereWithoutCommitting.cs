namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftWhereWithoutCommitting
    {
        private ITestHarness testHarness;

         async  Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoftWhereWithoutCommitting));

            var carId = Guid.Parse("0ec06a89-308b-4a3e-9cfb-d223ff4cdc67");
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            //When
            await this.testHarness.DataStore.DeleteSoftWhere<Car>(car => car.id == carId);
        }

        [Fact]
        public async void ItShouldOnlyMakeTheChangesInSession()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}