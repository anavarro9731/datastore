namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession
    {
        private readonly Guid car1Id;
        private readonly Guid car2Id;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession));

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

            this.testHarness.DataStore.Create(car1).Wait();
            this.testHarness.DataStore.Create(car2).Wait();
            car2.Make = "BMW";
            this.testHarness.DataStore.Update(car2).Wait();

            Assert.Equal(1, this.testHarness.DataStore.QueuedOperations.Count(x => x is QueuedUpdateOperation<Car>));
            
            //When
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldChangeOnlyTheItemUpdated()
        {        
            Assert.Equal("Volvo", this.testHarness.DataStore.ReadActiveById<Car>(this.car1Id).Result.Make);
            Assert.Equal("BMW", this.testHarness.DataStore.ReadActiveById<Car>(this.car2Id).Result.Make);
        }
    }
}