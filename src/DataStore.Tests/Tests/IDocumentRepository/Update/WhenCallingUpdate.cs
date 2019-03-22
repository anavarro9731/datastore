namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdate
    {
        private readonly Guid carId;

        private readonly ITestHarness testHarness;

        private readonly Car udpatedCar;

        private readonly Car existingCar;

        public WhenCallingUpdate()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdate));

            this.carId = Guid.NewGuid();

            this.existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToSecondsEpochTime()
            };
            this.testHarness.AddToDatabase(this.existingCar);

            var existingCarFromDb = this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result;

            existingCarFromDb.Make = "Ford";

            //When
            this.udpatedCar = this.testHarness.DataStore.Update(existingCarFromDb).Result;
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        [Fact]
        public void ItShouldPersistChangesToTheDatabase()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is UpdateOperation<Car>));
            Assert.Null(this.testHarness.DataStore.QueuedOperations.SingleOrDefault(e => e is QueuedUpdateOperation<Car>));
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }

        [Fact]
        public void ItShouldUpdateTheModifiedDate()
        {
            Assert.NotEqual(this.existingCar.Modified.Date, this.udpatedCar.Modified.Date);
            Assert.NotEqual(this.existingCar.ModifiedAsMillisecondsEpochTime, this.udpatedCar.ModifiedAsMillisecondsEpochTime);
            Assert.Equal("Ford", this.testHarness.DataStore.ReadActiveById<Car>(this.carId).Result.Make);
        }
    }
}