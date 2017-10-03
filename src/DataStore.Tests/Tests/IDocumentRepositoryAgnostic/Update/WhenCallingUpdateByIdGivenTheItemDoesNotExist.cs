namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateByIdGivenTheItemDoesNotExist
    {
        private readonly Car result;

        private readonly ITestHarness testHarness;

        public WhenCallingUpdateByIdGivenTheItemDoesNotExist()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingUpdateByIdGivenTheItemDoesNotExist));

            var carId = Guid.NewGuid();

            //When
            this.result = this.testHarness.DataStore.UpdateById<Car>(carId, car => car.Make = "Whatever").Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldNotExecuteAnyUpdateOperations()
        {
            Assert.Null(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
        }

        [Fact]
        public void ItShouldReturnNull()
        {
            Assert.Null(this.result);
        }
    }
}