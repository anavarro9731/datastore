namespace DataStore.Tests.Tests.Other
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenUsingByPassRULimit
    {
        private Car originalCar;

        private ITestHarness testHarness;

        private Car updatedCar;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldNotThrowAnError()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenUsingByPassRULimit));

            this.originalCar = new Car
            {
                id = Guid.NewGuid(), Make = "Volvo"
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(this.originalCar);

            //When
            await this.testHarness.DataStore.UpdateWhere<Car>(
                                  car => car.Make == "Volvo",
                                  car => car.Make = "Maseratti",
                                  options => options.BypassRULimit("bypass"));

            this.updatedCar = (await this.testHarness.DataStore.DeleteWhere<Car>(car => car.Make == "Maseratti")).Single();

            await this.testHarness.DataStore.CommitChanges();

            this.updatedCar = (await this.testHarness.DataStore.Read<Car>(car => car.Make == "Maseratti", options => options.BypassRULimit("test"))).Single();
            
            Assert.Equal(this.originalCar.id, this.updatedCar.id);
            Assert.Equal("Maseratti", this.updatedCar.Make);
        }
    }
}