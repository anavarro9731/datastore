namespace DataStore.Tests.Tests.IDocumentRepository.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingCreateWithoutSettingAnId
    {
        private readonly ITestHarness testHarness;

        private readonly Car newCar;

        public WhenCallingCreateWithoutSettingAnId()
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
            Assert.NotEqual(Guid.Empty, this.testHarness.QueryDatabase<Car>().Single().Id);
        }

        [Fact]
        public void ItShouldSetAnIdOnTheReturnValue()
        {
            Assert.NotEqual(Guid.Empty,this.newCar.Id);
        }

        [Fact]
        public void ReturnValueIdAndDatabaseIdShouldMatch()
        {
            Assert.Equal(this.testHarness.QueryDatabase<Car>().Single().Id
                , this.newCar.Id);
        }


    }
}