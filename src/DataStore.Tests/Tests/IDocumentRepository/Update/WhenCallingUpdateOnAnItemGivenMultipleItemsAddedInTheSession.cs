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

    public class WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession
    {
        private Guid car1Id;

        private Guid car2Id;

        private string car2PostCommitEtag;

        private string car2PostCreatePreUpdateEtag;

        private string car2PostUpdateEtag;

        private string car2PreCreateEtag;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldChangeOnlyTheItemUpdated()
        {
            await Setup();
            Assert.Equal("Volvo", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car1Id)).Make);
            Assert.Equal("BMW", (await this.testHarness.DataStore.ReadActiveById<Car>(this.car2Id)).Make);
        }

        [Fact(Skip = "Not Implemented Yet")]
        public void ItShouldMakeSureThatAsObjectsCommitTheyDontAffectOtherObjectsEtags()
        {
        }

        [Fact]
        public async void ItShouldSetTheCorrectEtags()
        {
            await Setup();
            Assert.Null(this.car2PreCreateEtag);
            Assert.Equal("waiting to be committed", this.car2PostCreatePreUpdateEtag);
            Assert.Equal("waiting to be committed", this.car2PostUpdateEtag);
            Assert.True(Guid.TryParse(this.car2PostCommitEtag.Trim('"'), out var result));
        }

        private async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingUpdateOnAnItemGivenMultipleItemsAddedInTheSession));

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

            await this.testHarness.DataStore.Create(car1);

            var r1 = await this.testHarness.DataStore.Create(car2);
            this.car2PostCreatePreUpdateEtag = r1.Etag;

            car2.Make = "BMW";
            var r2 = await this.testHarness.DataStore.Update(car2);
            this.car2PostUpdateEtag = r2.Etag;

            Assert.Equal(1, this.testHarness.DataStore.QueuedOperations.Count(x => x is QueuedUpdateOperation<Car>));

            //When
            await this.testHarness.DataStore.CommitChanges();
            this.car2PostCommitEtag = r2.Etag;
        }
    }
}