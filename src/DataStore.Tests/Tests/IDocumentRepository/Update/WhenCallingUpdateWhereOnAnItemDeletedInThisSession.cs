namespace DataStore.Tests.Tests.IDocumentRepository.Update
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

    public class WhenCallingUpdateWhereOnAnItemDeletedInThisSession
    {
        private  Guid carId;

        private  IEnumerable<Car> results;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateWhereOnAnItemDeletedInThisSession));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });
            await this.testHarness.DataStore.DeleteHardById<Car>(this.carId);

            //When
            this.results = await this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford");
        }

        [Fact]
        public async void ItShouldConsiderThePreviousDeleteInAnyFutureQueriesInSession()
        {
            await Setup();
            //in this circumstance we have deleted the only thing we updated so there is no update required
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));

            //nothing should have been updated because it was already deleted.
            Assert.Empty(this.results);
            Assert.Equal("Volvo", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
        }

        [Fact]
        public async void ItShouldDeleteTheItemInSession()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedHardDeleteOperation<Car>));
        }
    }
}