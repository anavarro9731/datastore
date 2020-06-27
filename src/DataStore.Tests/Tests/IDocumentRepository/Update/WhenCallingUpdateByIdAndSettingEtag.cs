namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateByIdAndSettingEtag
    {
        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldThrowAConcurrencyException()
        {
            await Setup();
            var exception =
                await Assert.ThrowsAnyAsync<DBConcurrencyException>(async () => await this.testHarness.DataStore.CommitChanges());
            Assert.Contains(this.carId.ToString(), exception.Message);
            Assert.Contains(typeof(Car).FullName, exception.Message);
        }

        private Task Setup()

        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateByIdAndSettingEtag));

            this.carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            //When
            var somePreviousTag = Guid.NewGuid().ToString();
            return this.testHarness.DataStore.UpdateById<Car>(this.carId, car => car.Etag = somePreviousTag);
        }
    }
}