using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataStore.DataAccess.Impl.DocumentDb;
using DataStore.DataAccess.Interfaces;
using DataStore.DataAccess.Interfaces.Events;
using DataStore.DataAccess.Models.Config;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DataStore.Tests
{
    public interface ITestHarness
    {
        DataStore DataStore { get; }

        List<IDataStoreEvent> Events { get; }

        Task AddToDatabase<T>(T aggregate) where T : IAggregate;

        Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : IHaveSchema, IHaveAUniqueId;
    }

    public static class TestFunctions
    {
        public static ITestHarness GetTestHarness(string testName)
        {
            return GetInMemoryTestHarness();
            //return GetRemoteDocumentDbTestHarness(new DocumentDbSettings(
            //    "", 
            //    "", 
            //    testName, 
            //    ""));
        }

        private static ITestHarness GetInMemoryTestHarness()
        {
            return new InMemoryTestHarness();
        }

        private static ITestHarness GetRemoteDocumentDbTestHarness(DocumentDbSettings dbConfig)
        {
            ClearTestDatabase(dbConfig);

            var repo = new DocumentRepository(dbConfig);

            return new RemoteTestHarness(repo);
        }

        private static void ClearTestDatabase(DocumentDbSettings documentDbSettings)
        {
            var documentClient = new DocumentDbClientFactory(documentDbSettings).GetDocumentClient();
            var db = documentClient.CreateDatabaseQuery().ToList().First();
            var coll = documentClient.CreateDocumentCollectionQuery(db.CollectionsLink).ToList().SingleOrDefault(x => x.Id == documentDbSettings.DefaultCollectionName);

            if (coll != null)
            {
                var docs = documentClient.CreateDocumentQuery(coll.DocumentsLink);
                foreach (var doc in docs)
                    documentClient.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }
    }
}