namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardWhereWithoutCommitting
    {
        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardWhereWithoutCommitting()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardWhereWithoutCommitting));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    Id = carId,
                    Make = "Volvo"
                });

            //When
            this.testHarness.DataStore.DeleteHardWhere<Car>(car => car.Id == carId).Wait();
        }

        [Fact]
        public async void ItShouldOnlyMakeChangesInSession()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}