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

    public class WhenCallingDeleteHardByIdWithoutCommitting
    {
        private ITestHarness testHarness;

         async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteHardByIdWithoutCommitting));

            var carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = carId,
                    Make = "Volvo"
                });

            //When
            await this.testHarness.DataStore.DeleteHardById<Car>(carId);
        }

        [Fact]
        public async void ItShouldOnlyMakeChangesInSession()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is HardDeleteOperation<Car>));
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
            Assert.NotEmpty(this.testHarness.QueryDatabase<Car>());
            Assert.Empty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}