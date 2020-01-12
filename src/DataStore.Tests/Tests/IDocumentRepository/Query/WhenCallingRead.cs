namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Providers.CosmosDb;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenCallingRead
    {
        //TODO fix
        //public WhenCallingRead(ITestOutputHelper output)
        //{
        //   CosmosDbUtilities.WriteLine = output.WriteLine;
        //}

        private IEnumerable<Car> carsFromDatabase;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingRead));

            var activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = activeCarId,
                Make = "Volvo"
            };

            var inactiveCarId = Guid.NewGuid();
            var inactiveExistingCar = new Car
            {
                id = inactiveCarId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(activeExistingCar);
            this.testHarness.AddToDatabase(inactiveExistingCar);

            // When
    
           var stopwatch = new Stopwatch().Op(s => s.Start());
           
           this.carsFromDatabase = await this.testHarness.DataStore.Read<Car>(car => car.Make == "Volvo");
           
           CosmosDbUtilities.WriteLine($"Time of Read {stopwatch.ElapsedMilliseconds}(ms)");
        }

        [Fact]
        public async void ItShouldReturnAllItemsRegardlessOfActiveFlag()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car> && e.MethodCalled == nameof(DataStore.Read)));
            Assert.Equal(2, this.carsFromDatabase.Count());
        }
    }
}