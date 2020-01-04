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

    public class WhenCallingDeleteSoft
    {
        private  Guid carId;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingDeleteSoft));

            this.carId = Guid.NewGuid();
            var car = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };

            this.testHarness.AddToDatabase(car);

            //When
            await this.testHarness.DataStore.DeleteSoft(car);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldPersistChangesToTheDatabase()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car> && e.MethodCalled == nameof(DataStore.DeleteSoft)));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }
    }
}