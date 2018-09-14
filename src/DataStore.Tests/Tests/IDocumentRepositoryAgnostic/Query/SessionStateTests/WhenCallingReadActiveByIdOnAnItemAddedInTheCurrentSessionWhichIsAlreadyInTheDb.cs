namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSessionWhichIsAlreadyInTheDb
    {
        private readonly Car newCarFromSession;

        private readonly ITestHarness testHarness;

        private readonly Guid fordId;

        private Guid volvoId;

        public WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSessionWhichIsAlreadyInTheDb()
        {
            // Given
            this.testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSessionWhichIsAlreadyInTheDb));

            this.volvoId = Guid.NewGuid();
            
            this.testHarness.AddToDatabase(new Car
                {
                    id = this.volvoId,
                    Active = true,
                    Make = "Volvo"
                });

            this.testHarness.DataStore.Create(
                new Car
                {
                    id = this.volvoId,
                    Active = true,
                    Make = "Ford"
                }).Wait();

        }

        [Fact]
        public void ItShouldErrorWhenYouRunAnyQueryWhichReplaysEvents()
        {
            var ex = Assert.ThrowsAny<Exception>(() => this.testHarness.DataStore.ReadActiveById<Car>(this.volvoId).Result);

            Assert.Contains("bb0ddf49-ccce-4588-ae74-5724fcdb8638", ex.InnerException.Message);

        }
    }
}