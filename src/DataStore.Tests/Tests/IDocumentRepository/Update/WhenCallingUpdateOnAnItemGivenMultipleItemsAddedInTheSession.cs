namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession
    {
        private Guid car1Id;
        private Guid car2Id;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession));

            this.car1Id = Guid.NewGuid();
            var car1 = new Car
            {
                id = this.car1Id,
                Make = "Volvo"
            };

            this.car2Id = Guid.NewGuid();
            var car2 = new Car
            {
                id = this.car2Id,
                Make = "Saab"
            };

            await this.testHarness.DataStore.Create(car1);
            await this.testHarness.DataStore.Create(car2);
            car2.Make = "BMW";
            await this.testHarness.DataStore.Update(car2);

            Assert.Equal(1, this.testHarness.DataStore.QueuedOperations.Count(x => x is QueuedUpdateOperation<Car>));
            
            //When
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldChangeOnlyTheItemUpdated()
        {
            await Setup();
            Assert.Equal("Volvo", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car1Id)).Make);
            Assert.Equal("BMW", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car2Id)).Make);
        }
    }
}