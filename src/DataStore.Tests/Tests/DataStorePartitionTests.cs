using System;
using System.Linq;
using DataStore.Impl.DocumentDb.Config;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests
{
    [Collection(TestCollections.DataStoreTestCollection)]
    public class DataStorePartitionTests
    {
        [Fact]
        public async void
            Integration_WhenDocumentsAreCreatedWithClassNameAsThePartitionKey_ItShouldPutThemAllInTheSamePartition()
        {
            //Given
            var collectionName = nameof(
                Integration_WhenDocumentsAreCreatedWithClassNameAsThePartitionKey_ItShouldPutThemAllInTheSamePartition);

            var docDbCollectionSettings =
                DocDbCollectionSettings.Create(collectionName,
                    DocDbCollectionSettings.PartitionKeyTypeEnum.ClassName);

            var testHarness = TestHarnessFunctions.GetTestHarness(collectionName);

            //When
            for (var i = 0; i < 30; i++)
            {
                var car = new Car
                {
                    id = Guid.NewGuid()
                };
                car.Make = "Saab";

                await testHarness.DataStore.Create(car);
            }
            await testHarness.DataStore.CommitChanges();

            //HACK: runtime manual override
            docDbCollectionSettings.EnableCrossParitionQueries = false;

            //Then
            //this line should not throw a cross-partition error because they are all in the same partition
            Assert.Equal(30, testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Result.Count());
        }

        [Fact]
        public async void
            Integration_WhenDocumentsAreCreatedWithIdAsThePartitionKey_ItShouldPutThemAllInSeparatePartitions()
            //up to the max no. of partitions (e.g. 25)
        {
            //Given
            var collectionName = nameof(
                Integration_WhenDocumentsAreCreatedWithIdAsThePartitionKey_ItShouldPutThemAllInSeparatePartitions);
            var docDbCollectionSettings =
                DocDbCollectionSettings.Create(
                    collectionName,
                    DocDbCollectionSettings.PartitionKeyTypeEnum.Id);

            var testHarness = TestHarnessFunctions.GetTestHarness(collectionName);

            //When
            for (var i = 0; i < 30; i++)
            {
                var car = new Car
                {
                    id = Guid.NewGuid()
                };
                car.Make = "Volvo";

                await testHarness.DataStore.Create(car);
            }
            await testHarness.DataStore.CommitChanges();

            //Then
            Assert.Equal(30, testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Result.Count());

            try
            {
                //HACK: runtime manual override
                docDbCollectionSettings.EnableCrossParitionQueries = false;

                //this fails because QueryDatabase() is not partition aware 
                await testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active));

                Assert.True(false); //never hit this
            }
            catch (Exception e)
            {
                Assert.True(e.Message.Contains("x-ms-documentdb-query-enablecrosspartition"));
            }
        }
    }
}