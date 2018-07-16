namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
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
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingCreateWithoutSettingAnId));

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
            Assert.NotEqual(Guid.Empty, this.testHarness.QueryDatabase<Car>().Single().id);
        }

        [Fact]
        public void ItShouldSetAnIdOnTheReturnValue()
        {
            Assert.NotEqual(Guid.Empty,this.newCar.id);
        }

        [Fact]
        public void ReturnValueIdAndDatabaseIdShouldMatch()
        {
            Assert.Equal(this.testHarness.QueryDatabase<Car>().Single().id
                , this.newCar.id);
        }


    }
}