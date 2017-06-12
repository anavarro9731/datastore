using System;
using System.Linq;
using DataStore.Impl.DocumentDb.Config;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.AzureOnly
{
    [Collection(TestCollections.RunSerially)]
    public class WhenDocumentsAreCreatedWithIdAsThePartitionKey
    {
        public WhenDocumentsAreCreatedWithIdAsThePartitionKey()
        {
            //Given
            var collectionName = nameof(WhenDocumentsAreCreatedWithIdAsThePartitionKey);

            docDbCollectionSettings = DocDbCollectionSettings.Create(
                collectionName,
                DocDbCollectionSettings.PartitionKeyTypeEnum.Id);

            testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName, docDbCollectionSettings);

            //When
            for (var i = 0; i < 3; i++) //only three because they are expensive
            {
                var car = new Car
                {
                    id = Guid.NewGuid(),
                    Make = "Volvo"
                };

                testHarness.DataStore.Create(car).Wait();
            }
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly DocDbCollectionSettings docDbCollectionSettings;

        //[Fact]
        [Fact(Skip = "expensive")]
        public void ItShouldPutThemAllInSeparatePartitions()
            //up to the max no. of partitions (e.g. 25)
        {
            Assert.Equal(3, testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Count());

            try
            {
                //HACK: runtime manual override
                docDbCollectionSettings.EnableCrossParitionQueries = false;

                //this fails because QueryDatabase() is not partition aware 
                testHarness.QueryDatabase<Car>(query => Queryable.Where<Car>(query, x => x.Active));

                Assert.True(false); //never hit this
            }
            catch (Exception e)
            {
                //consider aggregate exceptions
                Assert.True(e.ToString().Contains("x-ms-documentdb-query-enablecrosspartition"));
            }
        }
    }
}