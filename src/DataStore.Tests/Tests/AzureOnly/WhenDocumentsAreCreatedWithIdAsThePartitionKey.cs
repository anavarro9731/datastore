namespace DataStore.Tests.Tests.AzureOnly
{
    using System;
    using System.Linq;
    using global::DataStore.Impl.DocumentDb.Config;
    using global::DataStore.Tests.Constants;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    [Collection(TestCollections.RunSerially)]
    public class WhenDocumentsAreCreatedWithIdAsThePartitionKey
    {
        private readonly DocDbCollectionSettings docDbCollectionSettings;

        private readonly ITestHarness testHarness;

        public WhenDocumentsAreCreatedWithIdAsThePartitionKey()
        {
            //Given
            var collectionName = nameof(WhenDocumentsAreCreatedWithIdAsThePartitionKey);

            this.docDbCollectionSettings = DocDbCollectionSettings.Create(collectionName, DocDbCollectionSettings.PartitionKeyTypeEnum.Id);

            this.testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName, this.docDbCollectionSettings);

            //When
            for (var i = 0; i < 3; i++) //only three because they are expensive
            {
                var car = new Car
                {
                    id = Guid.NewGuid(),
                    Make = "Volvo"
                };

                this.testHarness.DataStore.Create(car).Wait();
            }
            this.testHarness.DataStore.CommitChanges().Wait();
        }

        //[Fact]
        [Fact(Skip = "azure only")]
        public void ItShouldPutThemAllInSeparatePartitions()
            //up to the max no. of partitions (e.g. 25)
        {
            Assert.Equal(3, this.testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Count());

            try
            {
                //HACK: runtime manual override
                this.docDbCollectionSettings.EnableCrossParitionQueries = false;

                //this fails because QueryDatabase() is not partition aware 
                this.testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active));

                Assert.True(false); //never hit this
            }
            catch (Exception e)
            {
                //consider aggregate exceptions
                Assert.Contains("x-ms-documentdb-query-enablecrosspartition", e.ToString());
            }
        }
    }
}