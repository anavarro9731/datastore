namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteSoftByIdWithoutCommitting
    {
        private readonly ITestHarness testHarness;

        public WhenCallingDeleteSoftByIdWithoutCommitting()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoftByIdWithoutCommitting));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            //When
            this.testHarness.DataStore.DeleteSoftById<Car>(carId).Wait();
        }

        [Fact]
        public async void ItShouldOnlyMakeChangesInSession()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}