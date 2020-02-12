namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateByIdAndSettingEtag
    {
        private  Guid carId;

        private  ITestHarness testHarness;

        Task Setup()

        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateByIdAndSettingEtag));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            //When
            var somePreviousTag = Guid.NewGuid().ToString();
            return this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Etag = somePreviousTag);
        }

        [Fact]
        public async void ItShouldThrowAConcurrencyException()
        {
            await Setup();
            var exception = await Assert.ThrowsAnyAsync<DBConcurrencyException>(async () => await this.testHarness.DataStore.CommitChanges());
            Assert.Contains(this.carId.ToString(), exception.Message);
            Assert.Contains(typeof(Car).FullName, exception.Message);
        }
    }
}