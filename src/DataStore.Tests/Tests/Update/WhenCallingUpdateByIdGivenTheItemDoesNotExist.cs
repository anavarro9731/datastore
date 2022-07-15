namespace DataStore.Tests.Tests.Update
{
    #region

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    #endregion

    public class WhenCallingUpdateByIdGivenTheItemDoesNotExist
    {
        private Car result;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotExecuteAnyUpdateOperations()
        {
            await Setup();
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(this.result);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateByIdGivenTheItemDoesNotExist));

            var carId = Guid.NewGuid();

            //When
            this.result = await this.testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Whatever");
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}