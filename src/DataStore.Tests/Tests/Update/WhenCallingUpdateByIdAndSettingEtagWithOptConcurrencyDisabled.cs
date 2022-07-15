namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdateByIdAndSettingEtagWithOptConcurrencyDisabled
    {
        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotThrowAConcurrencyException()
        {
            await Setup();
            await this.testHarness.DataStore.CommitChanges();
        }

        private Task Setup()

        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateByIdAndSettingEtagWithOptConcurrencyDisabled),
                DataStoreOptions.Create().DisableOptimisticConcurrency());

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