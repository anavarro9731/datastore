namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingDeleteHardById
    {
        private readonly Guid carId;

        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingDeleteHardById()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingDeleteHardById));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            //When
            this.result = this.testHarness.DataStore.DeleteHardById<Car>(this.carId).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public async void ItShouldFlushTheSessionCache()
        {
            Assert.Empty(this.testHarness.DataStore.QueuedOperations);
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.Empty(this.testHarness.QueryDatabase<Car>());
        }

        [Fact]
        public void ItShouldReturnTheItemDeleted()
        {
            Assert.Equal(this.carId, this.result.id);
        }
    }
}