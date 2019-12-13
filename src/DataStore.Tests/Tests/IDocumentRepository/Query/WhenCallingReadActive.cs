namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActive
    {
        private Car activeCarFromDatabase;

        private Car inactiveCarFromDatabase;

        async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActive));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Jeep"
            };
            testHarness.AddToDatabase(activeExistingCar);
            testHarness.AddToDatabase(inactiveExistingCar);

            // When
            this.activeCarFromDatabase = (await testHarness.DataStore.ReadActive<Car>(car => car.id == activeCarId)).SingleOrDefault();
            this.inactiveCarFromDatabase = (await testHarness.DataStore.ReadActive<Car>(car => car.id == inactiveCarId)).SingleOrDefault();
        }

        [Fact]
        public async void ItShouldNotReturnInActiveItems()
        {
            await Setup();
            Assert.Null(this.inactiveCarFromDatabase);
        }

        [Fact]
        public async void ItShouldReturnActiveItems()
        {
            await Setup();
            Assert.Equal("Volvo", this.activeCarFromDatabase.Make);
        }
    }
}