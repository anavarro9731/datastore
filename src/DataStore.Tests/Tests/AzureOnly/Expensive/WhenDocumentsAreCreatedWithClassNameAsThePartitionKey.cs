using System;
using System.Linq;
using DataStore.Impl.DocumentDb.Config;
using DataStore.Tests.Constants;
using DataStore.Tests.Models;
using DataStore.Tests.TestHarness;
using Xunit;

namespace DataStore.Tests.Tests.AzureOnly.Expensive
{
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

            testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName, docDbCollectionSettings);

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
}