namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.Tests.TestHarness;
    using Xunit;

    public class WhenCallingUpdateOnAnItemGivenMultipleItemsOfTheSameTypeAddedInTheSession
    {
        private Guid car1Id;

        private string car1PostCommitEtag;

        private string car1PostCreateEtag;

        private string car1PreCreateEtag;

        private Guid car2Id;

        private string car2PostCommitEtag;

        private string car2PostCreatePreUpdateEtag;

        private string car2PostUpdateEtag;

        private string car2PreCreateEtag;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldMakeSureThatAsObjectsCommitTheyDontAffectOtherObjectsEtags()
        {
            await Setup();
            Assert.NotNull(this.car1PostCommitEtag);
            Assert.NotEqual(this.car1PostCommitEtag, this.car2PostCommitEtag);
        }

        [Fact]
        public async void ItShouldSetTheCorrectEtagsOnCar1DuringTheSession()
        {
            await Setup();
            Assert.Null(this.car2PreCreateEtag); //* has not been set
            Assert.Null(this.car1PreCreateEtag); //* has not been set

            Assert.Equal("waiting to be committed", this.car1PostCreateEtag);
            Assert.Equal("waiting to be committed", this.car2PostCreatePreUpdateEtag);
            Assert.Equal("waiting to be committed", this.car2PostUpdateEtag);

            Assert.True(Guid.TryParse(this.car2PostCommitEtag.Trim('"'), out var result1)); //* be a guid
            Assert.True(Guid.TryParse(this.car1PostCommitEtag.Trim('"'), out var result2)); //* be a guid
        }

        [Fact]
        public async void ItShouldUpdateTheSecondCar()
        {
            await Setup();
            Assert.Equal("Volvo", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car1Id)).Make);
            Assert.Equal("BMW", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car2Id)).Make);
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateOnAnItemGivenMultipleItemsOfTheSameTypeAddedInTheSession));

            this.car1Id = Guid.NewGuid();
            var car1 = new Car
            {
                id = this.car1Id, Make = "Volvo"
            };

            this.car2Id = Guid.NewGuid();
            var car2 = new Car
            {
                id = this.car2Id, Make = "Saab"
            };

            this.car2PreCreateEtag = car2.Etag;
            this.car1PreCreateEtag = car1.Etag;
            
            //* create CAR 1
            var c1r1 = await this.testHarness.DataStore.Create(car1);
            this.car1PostCreateEtag = c1r1.Etag;

            //* create CAR 2
            var c2r1 = await this.testHarness.DataStore.Create(car2);
            this.car2PostCreatePreUpdateEtag = c2r1.Etag;

            //* update CAR 2  
            car2.Make = "BMW";
             var c2r2 = await this.testHarness.DataStore.Update(car2);  /* this line in this test does a very important job it test the failure in the update method 
             of the developer adding but forgetting to add a new restricted property on the Aggregate base class to the list of excluded properties when cloning 
             during the Update() method */
             
             this.car2PostUpdateEtag = c2r2.Etag;

            Assert.Equal(0, this.testHarness.DataStore.QueuedOperations.Count(x => x is QueuedUpdateOperation<Car>));
            Assert.Equal(2, this.testHarness.DataStore.QueuedOperations.Count(x => x is QueuedCreateOperation<Car>));

            //When
            await this.testHarness.DataStore.CommitChanges();
            this.car1PostCommitEtag = c1r1.Etag;
            this.car2PostCommitEtag = c2r2.Etag;
        }
    }
}