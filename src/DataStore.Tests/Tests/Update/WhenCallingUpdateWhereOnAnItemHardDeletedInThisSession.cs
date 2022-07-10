namespace DataStore.Tests.Tests.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWhereOnAnItemHardDeletedInThisSession
    {
        private Guid carId;

        private IEnumerable<Car> results;

        private ITestHarness testHarness;


        [Fact]
        public async void ItShouldNotAttemptAnUpdate()
        {
            await Setup();
            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal(
                "Volvo",
                this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);

        }

        [Fact]
        public async void ItShouldNotReturnAnyResults()
        {
            await Setup();
            //nothing should have been updated because it was already deleted.
            Assert.Empty(this.results);

        }

        [Fact]
        public async void ItShouldDeleteTheItemInSession()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWhereOnAnItemHardDeletedInThisSession));

            this.carId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });
            await this.testHarness.DataStore.DeleteById<Car>(this.carId, o => o.Permanently());

            //When
            this.results = await this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford");
        }
    }
}