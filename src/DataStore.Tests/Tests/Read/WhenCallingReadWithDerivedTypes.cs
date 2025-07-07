namespace DataStore.Tests.Tests.Read
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Newtonsoft.Json;
    using Xunit;

    #endregion

    public class WhenCallingReadWithDerivedTypes
    {
        private IEnumerable<Car> carsFromDatabase;

        private ITestHarness testHarness;
        private Guid newCarId;
        
        [Fact]
        public async void ItShouldReturnTheDerivedType()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            
            await Setup();
            Assert.True(this.carsFromDatabase.Single().Wheels.Exists(x => x is Car.FancyWheel));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithDerivedTypes));

            this.newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                id = this.newCarId, Make = "Volvo",
                Wheels = new List<Car.Wheel>()
                {
                    new Car.Wheel() { FriendlyId = "W1"},
                    new Car.FancyWheel() { FriendlyId = "F1", Coating = "Enamel"}
                }
            };

            await this.testHarness.DataStore.Create(newCar);
            await this.testHarness.DataStore.CommitChanges();
            // When

            this.carsFromDatabase = await this.testHarness.DataStore.Read<Car>(car => car.Make == "Volvo");
        }
    }
}