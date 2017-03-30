using DataStore.Impl.DocumentDb.Config;

namespace DataStore.Tests.Tests
{
    using System;
    using System.Linq;
    using Constants;
    using Models;
    using TestHarness;
    using Xunit;

    [Collection(TestCollections.DataStoreTestCollection)]
    public class DataStorePartitionTests
    {
        [Fact]
        public async void
            Integration_WhenDocumentsAreCreatedWithClassNameAsThePartitionKey_ItShouldPutThemAllInTheSamePartition()
        {
            //Given
            var docDbCollectionSettings =
                DocDbCollectionSettings.Create(
                    nameof(
                        Integration_WhenDocumentsAreCreatedWithClassNameAsThePartitionKey_ItShouldPutThemAllInTheSamePartition),
                    DocDbCollectionSettings.PartitionKeyTypeEnum.ClassName);

            var testHarness = TestHarnessFunctions.GetTestHarness(
                TestHarnessOptions.Create(docDbCollectionSettings));

            //When
            for (var i = 0; i < 30; i++)
            {
                var car = new Car
                {
                    Id = Guid.NewGuid()
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
            var docDbCollectionSettings =
                DocDbCollectionSettings.Create(
                    nameof(
                        Integration_WhenDocumentsAreCreatedWithIdAsThePartitionKey_ItShouldPutThemAllInSeparatePartitions),
                    DocDbCollectionSettings.PartitionKeyTypeEnum.Id);

            var testHarness = TestHarnessFunctions.GetTestHarness(
                TestHarnessOptions.Create(docDbCollectionSettings));

            //When
            for (var i = 0; i < 30; i++)
            {
                var car = new Car
                {
                    Id = Guid.NewGuid()
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