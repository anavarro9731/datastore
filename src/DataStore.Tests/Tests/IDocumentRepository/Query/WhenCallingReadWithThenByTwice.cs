namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using global::DataStore.Interfaces;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithThenByTwice
    {
        private Guid firstCarId;

        private Guid fourthCarId;

        private Guid secondCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        [Fact]
        public async void ItShouldNotThrowAnErrorDueToDuplicateIndexes()
        {
            Setup();
            // When
            await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                                       setOptions: o => o.OrderBy(c => c.Make).ThenBy(c => c.Active, true).ThenBy(c => c.id));

            await this.testHarness.DataStore.WithoutEventReplay.Read<Car>(
                setOptions: o => o.OrderBy(c => c.Make).ThenBy(c => c.Active, true).ThenBy(c => c.id));

        }

        private void Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithThenByTwice));

            this.firstCarId = Guid.Parse("c74bbd8f-b9c4-4529-ba55-6b920c4b4a42");
            var firstExistingCar = new Car
            {
                id = this.firstCarId, Make = "Volvo", Active = true, Year = 2011
            };

            this.secondCarId = Guid.Parse("ae9dea20-538c-44ab-b372-9bd2e7ddd1c8");
            var secondExistingCar = new Car
            {
                id = this.secondCarId, Active = false, Make = "Volvo", Year = 2010
            };

            this.thirdCarId = Guid.Parse("fac65251-261a-4c6e-b13c-0d9d80e2b761");
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId, Active = false, Make = "Ford", Year = 2010
            };

            this.fourthCarId = Guid.Parse("34f15cd6-cbbf-4d69-bce3-8eecb8dce138");
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId, Active = false, Make = "Volvo", Year = 2010
            };

            this.testHarness.AddItemDirectlyToUnderlyingDb(firstExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(secondExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(thirdExistingCar);
            this.testHarness.AddItemDirectlyToUnderlyingDb(fourthExistingCar);

            //note with booleans, true has a higher sort order
            //you can also try with Year to test ints
        }
    }
}
