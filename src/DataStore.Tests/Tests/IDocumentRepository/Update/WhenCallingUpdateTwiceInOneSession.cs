namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateTwiceInOneSession
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateTwiceInOneSession()
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
            this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Toyota").Wait();
            this.testHarness.DataStore.UpdateById<Car>(this.carId, c => c.Make = "Honda").Wait();
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldCallUpdateTwice()
        {
            Assert.Equal(2, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldPersistTheLastChangeToTheDatabase()
        {
            Assert.Equal("Honda", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}