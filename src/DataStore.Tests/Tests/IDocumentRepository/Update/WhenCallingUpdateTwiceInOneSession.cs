namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateTwiceInOneSession
    {
        private Guid carId;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateTwiceInOneSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            //When
            await this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Toyota");
            await this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Honda");
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldCallUpdateTwice()
        {
            await Setup();
            Assert.Equal(2, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldPersistTheLastChangeToTheDatabase()
        {
            await Setup();
            Assert.Equal("Honda", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }
    }
}