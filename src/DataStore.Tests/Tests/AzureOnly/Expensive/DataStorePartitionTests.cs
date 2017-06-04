namespace DataStore.Tests.Tests.AzureOnly.Expensive
{
    using System;
    using System.Linq;
    using Constants;
    using Impl.DocumentDb.Config;
    using Models;
    using TestHarness;
    using Xunit;

    [Collection(TestCollections.RunSerially)]
    public class WhenDocumentsAreCreatedWithClassNameAsThePartitionKey
    {
        public WhenDocumentsAreCreatedWithClassNameAsThePartitionKey()
        {
            //Given
            var collectionName = nameof(WhenDocumentsAreCreatedWithClassNameAsThePartitionKey);

            var docDbCollectionSettings =
                DocDbCollectionSettings.Create(collectionName,
                    DocDbCollectionSettings.PartitionKeyTypeEnum.ClassName);

            testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName);

            //When
            for (var i = 0; i < 30; i++)
            {
                var car = new Car
                {
                    id = Guid.NewGuid(),
                    Make = "Saab"
                };

                testHarness.DataStore.Create(car).Wait();
            }
            testHarness.DataStore.CommitChanges().Wait();

            //HACK: runtime manual override
            docDbCollectionSettings.EnableCrossParitionQueries = false;
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public void ItShouldPutThemAllInTheSamePartition()
        {
            //this line should not throw a cross-partition error because they are all in the same partition
            Assert.Equal(30, testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Count());
        }
    }

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

            testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName);

            //When
            for (var i = 0; i < 30; i++)
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

        [Fact]
        public void ItShouldPutThemAllInSeparatePartitions()
            //up to the max no. of partitions (e.g. 25)
        {
            Assert.Equal(30, testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Count());

            try
            {
                //HACK: runtime manual override
                docDbCollectionSettings.EnableCrossParitionQueries = false;

                //this fails because QueryDatabase() is not partition aware 
                testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active));

                Assert.True(false); //never hit this
            }
            catch (Exception e)
            {
                Assert.True(e.Message.Contains("x-ms-documentdb-query-enablecrosspartition"));
            }
        }
    }
}