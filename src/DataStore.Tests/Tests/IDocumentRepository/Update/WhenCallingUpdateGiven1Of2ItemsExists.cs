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

    public class WhenCallingUpdateGiven1Of2ItemsExists
    {
        private  IEnumerable<Car> result;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateGiven1Of2ItemsExists));

            var volvoId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = volvoId,
                    Make = "Volvo"
                });

            var fordId = Guid.NewGuid();
            this.testHarness.AddItemDirectlyToUnderlyingDb(
                new Car
                {
                    id = fordId,
                    Make = "Ford"
                });

            //When
            this.result = await this.testHarness.DataStore.UpdateWhere<Car>(c => c.Make == "Volvo", car => car.Year = 2000);
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldExecuteOnlyOneUpdate()
        {
            await Setup();
            Assert.Equal(1, this.testHarness.DataStore.ExecutedOperations.Count(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public async void ItShouldReturnOnlyOneItem()
        {
            await Setup();
            Assert.Single(this.result);
        }
    }
}