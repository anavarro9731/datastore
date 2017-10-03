namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
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
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteSoftByIdWithoutCommitting));

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
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is SoftDeleteOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedSoftDeleteOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}