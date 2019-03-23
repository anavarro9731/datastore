namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithoutSettingAnId
    {
        private  ITestHarness testHarness;

        private  Car newCar;

        async Task Setup()
        {
            
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithoutSettingAnId));
            
            this.newCar = new Car
            {
                Make = "Volvo"
            };

            //When
            this.newCar = await this.testHarness.DataStore.Create(this.newCar);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldSetAnIdOnTheNewlyCreatedItemInTheDatabase()
        {
            await Setup();
            Assert.NotEqual(Guid.Empty, this.testHarness.QueryDatabase<Car>().Single().id);
        }

        [Fact]
        public async void ItShouldSetAnIdOnTheReturnValue()
        {
            await Setup();
            Assert.NotEqual(Guid.Empty,this.newCar.id);
        }

        [Fact]
        public async void ReturnValueIdAndDatabaseIdShouldMatch()
        {
            await Setup();
            Assert.Equal(this.testHarness.QueryDatabase<Car>().Single().id
                , this.newCar.id);
        }


    }
}