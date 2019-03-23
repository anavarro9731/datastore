namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithoutSettingAnId
    {
        private  ITestHarness testHarness;

        private  Car newCar;

        void Setup()
        {
            
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingCreateWithoutSettingAnId));
            
            this.newCar = new Car
            {
                Make = "Volvo"
            };

            //When
            this.newCar = this.testHarness.DataStore.Create(this.newCar).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldSetAnIdOnTheNewlyCreatedItemInTheDatabase()
        {
            Setup();
            Assert.NotEqual(Guid.Empty, this.testHarness.QueryDatabase<Car>().Single().id);
        }

        [Fact]
        public void ItShouldSetAnIdOnTheReturnValue()
        {
            Setup();
            Assert.NotEqual(Guid.Empty,this.newCar.id);
        }

        [Fact]
        public void ReturnValueIdAndDatabaseIdShouldMatch()
        {
            Setup();
            Assert.Equal(this.testHarness.QueryDatabase<Car>().Single().id
                , this.newCar.id);
        }


    }
}