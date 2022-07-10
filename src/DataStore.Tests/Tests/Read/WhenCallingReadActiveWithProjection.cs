namespace DataStore.Tests.Tests.Read
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithProjection
    {
        private Guid secondCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        [Fact]
        public async void ItShouldReturnTheProjection()
        {
            Setup();

            var cars = await this.testHarness.DataStore.WithoutEventReplay.ReadActive<Car, CarProjection>(
                           car => new CarProjection
                           {
                               Make = car.Make, CarId = car.id
                           },
                           x => x.Make == "Volvo",
                           options => options.OrderBy(x => x.Make));

            var carProjections = cars as CarProjection[] ?? cars.ToArray();
            Assert.Single(carProjections);
            Assert.IsType<CarProjection>(carProjections.First());
            Assert.Equal("Volvo", carProjections.First().Make);
            Assert.NotEqual(Guid.Empty, carProjections.First().CarId);
            Assert.NotEqual(DateTime.MinValue, carProjections.First().Created);
        }

        private void Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveWithProjection));

            var activeCarId = Guid.Parse("c74bbd8f-b9c4-4529-ba55-6b920c4b4a42");
            var activeExistingCar = new Car
            {
                id = activeCarId, 
                Make = "Volvo",
                Created = DateTime.UtcNow,
                CreatedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime(),
                Active = true,
                Wheels = new List<Car.Wheel>()
                {
                    new Car.Wheel()
                    {
                        RimSize = 15
                    }
                }
            };

            this.secondCarId = Guid.Parse("ae9dea20-538c-44ab-b372-9bd2e7ddd1c8");
            var thirdExistingCar = new Car
            {
                id = this.secondCarId, 
                Make = "Volvo",
                Created = DateTime.UtcNow,
                CreatedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime(),
                Active = false,
                Wheels = new List<Car.Wheel>()
                {
                    new Car.Wheel()
                    {
                        RimSize = 18
                    }
                }
            };

            this.thirdCarId = Guid.Parse("fac65251-261a-4c6e-b13c-0d9d80e2b761");
            var fourthExistingCar = new Car
            {
                id = this.thirdCarId, Active = true, Make = "Toyota"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(activeExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(thirdExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(fourthExistingCar);
        }

        public class CarProjection : Aggregate
        {
            public Guid CarId { get; set; }

            public string Make { get; set; }
        }
    }
}