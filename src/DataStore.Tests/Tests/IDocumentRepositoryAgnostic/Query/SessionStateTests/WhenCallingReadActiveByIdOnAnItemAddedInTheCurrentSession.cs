namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession
    {
        public WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = true,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            var newCarId = Guid.NewGuid();
            testHarness.DataStore.Create(new Car
                {
                    id = newCarId,
                    Active = true,
                    Make = "Ford"
                })
                .Wait();


            newCarFromSession = testHarness.DataStore.ReadActiveById<Car>(newCarId).Result;
        }

        private readonly ITestHarness testHarness;
        private readonly Car newCarFromSession;

        [Fact]
        public void ItShouldReturnThatItem()
        {
            Assert.NotNull(testHarness.Operations.All(e => e is AggregateQueriedByIdOperation));                      
            Assert.NotNull(newCarFromSession);
        }

        [Fact]
        public void ItShouldNotHaveAddedThatItemToTheDatabaseYet()
        {
            Assert.Equal(1, testHarness.QueryDatabase<Car>().Count());
            Assert.Equal(2, testHarness.DataStore.ReadActive<Car>().Result.Count());
        }
    }
}