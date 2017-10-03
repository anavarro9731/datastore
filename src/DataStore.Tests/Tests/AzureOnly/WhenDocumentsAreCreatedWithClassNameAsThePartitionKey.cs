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
    public class WhenDocumentsAreCreatedWithClassNameAsThePartitionKey
    {
        private readonly ITestHarness testHarness;

        public WhenDocumentsAreCreatedWithClassNameAsThePartitionKey()
        {
            //Given
            var collectionName = nameof(WhenDocumentsAreCreatedWithClassNameAsThePartitionKey);

            var docDbCollectionSettings = DocDbCollectionSettings.Create(collectionName, DocDbCollectionSettings.PartitionKeyTypeEnum.ClassName);

            this.testHarness = TestHarnessFunctions.GetDocumentDbTestHarness(collectionName, docDbCollectionSettings);

            //When
            for (var i = 0; i < 5; i++)
            {
                var car = new Car
                {
                    id = Guid.NewGuid(),
                    Make = "Saab"
                };

                this.testHarness.DataStore.Create(car).Wait();
            }
            this.testHarness.DataStore.CommitChanges().Wait();

            //HACK: runtime manual override
            docDbCollectionSettings.EnableCrossParitionQueries = false;
        }

        //[Fact]
        [Fact(Skip = "azure only")]
        public void ItShouldPutThemAllInTheSamePartition()
        {
            //this line should not throw a cross-partition error because they are all in the same partition
            Assert.Equal(5, this.testHarness.QueryDatabase<Car>(query => query.Where(x => x.Active)).Count());
        }
    }
}