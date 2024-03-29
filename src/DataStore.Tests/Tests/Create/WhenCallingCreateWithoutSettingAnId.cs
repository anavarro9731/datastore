namespace DataStore.Tests.Tests.Create
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingCreateWithoutSettingAnId
    {
        private Car newCar;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldSetAnIdOnTheNewlyCreatedItemInTheDatabase()
        {
            await Setup();
            Assert.NotEqual(Guid.Empty, this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().id);
        }

        [Fact]
        public async void ItShouldSetAnIdOnTheNewlyCreatedItemsChildEntitiesInTheDatabase()
        {
            await Setup();
            var result = this.testHarness.QueryUnderlyingDbDirectly<Car>().Single();
            Assert.NotEqual(Guid.Empty, result.Wheels.First().id);
        }

        [Fact]
        public async void ItShouldSetAnIdOnTheReturnValue()
        {
            await Setup();
            Assert.NotEqual(Guid.Empty, this.newCar.id);
        }

        [Fact]
        public async void ReturnValueIdAndDatabaseIdShouldMatch()
        {
            await Setup();
            Assert.Equal(this.testHarness.QueryUnderlyingDbDirectly<Car>().Single().id, this.newCar.id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithoutSettingAnId));

            this.newCar = new Car
            {
                Make = "Volvo",
                Wheels = new[]
                {
                    new Car.Wheel(), new Car.Wheel(), new Car.Wheel(), new Car.Wheel()
                }.ToList()
            };

            //When
            this.newCar = await this.testHarness.DataStore.Create(this.newCar);
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}