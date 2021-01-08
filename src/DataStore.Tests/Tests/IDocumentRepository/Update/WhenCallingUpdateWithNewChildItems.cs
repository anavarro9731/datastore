namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateWithNewChildItems
    {
        private const string thWheel = "5th Wheel";

        private readonly Guid unitOfWorkId = Guid.NewGuid();

        private Guid carId;

        private Car existingCar;

        private ITestHarness testHarness;

        private Car udpatedCar;

        private List<Aggregate.AggregateVersionInfo> versionHistory;

        [Fact]
        public async void ItShouldSetTheIdOnTheNewWheel()
        {
            await Setup();

            Assert.NotEqual(
                Guid.Empty,
                (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Wheels.Single(w => w.FriendlyId == thWheel).id);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(
                nameof(WhenCallingUpdateWithNewChildItems),
                DataStoreOptions.Create().SpecifyUnitOfWorkId(this.unitOfWorkId));

            this.carId = Guid.NewGuid();

            this.existingCar = new Car
            {
                id = this.carId,
                Make = "Volvo",
                Modified = DateTime.UtcNow.AddDays(-1),
                ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToMillisecondsEpochTime()
            };
            this.testHarness.AddItemDirectlyToUnderlyingDb(this.existingCar);

            var existingCarFromDb = await this.testHarness.DataStore.ReadActiveById<Car>(this.carId);

            existingCarFromDb.Wheels.Add(
                new Car.Wheel
                {
                    RimSize = 5, FriendlyId = thWheel
                });

            //When
            this.udpatedCar = await this.testHarness.DataStore.Update(existingCarFromDb);
            await this.testHarness.DataStore.CommitChanges();

            this.versionHistory = this.testHarness.QueryUnderlyingDbDirectly<Car>(cars => cars.Where(c => c.id == this.carId)).Single()
                                      .VersionHistory;
        }
    }
}
