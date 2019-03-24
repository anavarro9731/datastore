namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadWithThenBy
    {

        private Guid firstCarId;

        private Guid secondCarId;

        private ITestHarness testHarness;

        private Guid thirdCarId;

        private Guid fourthCarId;

        void Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadWithThenBy));

            this.firstCarId = Guid.Parse("c74bbd8f-b9c4-4529-ba55-6b920c4b4a42");
            var firstExistingCar = new Car
            {
                id = this.firstCarId,
                Make = "Volvo",
                Active = true,
                Year = 2011
            };

            this.secondCarId = Guid.Parse("ae9dea20-538c-44ab-b372-9bd2e7ddd1c8");
            var secondExistingCar = new Car
            {
                id = this.secondCarId,
                Active = false,
                Make = "Volvo",
                Year = 2010
            };

            this.thirdCarId = Guid.Parse("fac65251-261a-4c6e-b13c-0d9d80e2b761");
            var thirdExistingCar = new Car
            {
                id = this.thirdCarId,
                Active = false,
                Make = "Ford",
                Year = 2010
            };

            this.fourthCarId = Guid.Parse("34f15cd6-cbbf-4d69-bce3-8eecb8dce138");
            var fourthExistingCar = new Car
            {
                id = this.fourthCarId,
                Active = false,
                Make = "Volvo",
                Year = 2010
            };

            this.testHarness.AddToDatabase(firstExistingCar);
            this.testHarness.AddToDatabase(secondExistingCar);
            this.testHarness.AddToDatabase(thirdExistingCar);
            this.testHarness.AddToDatabase(fourthExistingCar);


            //note with booleans, true has a higher sort order
            //you can also try with Year to test ints
        }

        [Fact]
        public async void ItShouldReturnTheCarsInTheRightOrderForStringsAndIntsAndGuids()
        {
            Setup();
            // When
            var carsFromDatabase =
                await this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(
                    o => o.OrderBy(c => c.Make).ThenBy(c => c.Active, true).ThenBy(c => c.id));

            Assert.Equal(4, carsFromDatabase.Count());
            Assert.Equal(this.thirdCarId, carsFromDatabase.First().id);
            Assert.Equal(this.firstCarId, carsFromDatabase.Skip(1).First().id);
            Assert.Equal(this.fourthCarId, carsFromDatabase.Skip(2).First().id);
            Assert.Equal(this.secondCarId, carsFromDatabase.Last().id);
        }

        [Fact]
        public async void ItShouldReturnTheCarsInTheRightOrderForStringsAndBooleansAndGuids()
        {
            Setup();
            // When
            var carsFromDatabase =await 
                this.testHarness.DataStore.WithoutEventReplay.Read<Car, WithoutReplayOptions<Car>>(
                    o => o.OrderBy(c => c.Make).ThenBy(c => c.Active, true).ThenBy(c => c.id));

            Assert.Equal(4, carsFromDatabase.Count());
            Assert.Equal(this.thirdCarId, carsFromDatabase.First().id);
            Assert.Equal(this.firstCarId, carsFromDatabase.Skip(1).First().id);
            Assert.Equal(this.fourthCarId, carsFromDatabase.Skip(2).First().id);
            Assert.Equal(this.secondCarId, carsFromDatabase.Last().id);
        }

        //TODO Test Dates
    }
}