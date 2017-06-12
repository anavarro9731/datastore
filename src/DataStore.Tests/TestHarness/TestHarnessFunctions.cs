using System;
using System.IO;
using DataStore.Impl.DocumentDb.Config;
using DataStore.Impl.SqlServer;
using Newtonsoft.Json;

namespace DataStore.Tests.TestHarness
{
    public static class TestHarnessFunctions
    {
        public static ITestHarness GetTestHarness(string testName)
        {
            //return GetDocumentDbTestHarness(testName);
            //return GetSqlServerTestHarness(testName);
            return GetInMemoryTestHarness();
        }

        internal static ITestHarness GetDocumentDbTestHarness(string testName, DocDbCollectionSettings collectionSettings = null)
        {
            var options = TestHarnessOptions.Create(collectionSettings ?? DocDbCollectionSettings.Create(testName));

            var documentdbsettingsJson = "DocumentDbSettings.json";
            /*
            Create this file in your DataStore.Tests project root and set it's build output to "copy always"
            This file should always be .gitignore(d), don't want to expose this in your repo.
            {
                "AuthorizationKey": "<azurekey>",
                "DatabaseName": "<dbname>",
                "EndpointUrl": "<endpointurl>"
            }
            */
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, documentdbsettingsJson);

            var documentDbSettings = JsonConvert.DeserializeObject<DocumentDbSettings>(File.ReadAllText(location));

            documentDbSettings.CollectionSettings = options.CollectionSettings;

            return DocumentDbTestHarness.Create(documentDbSettings);
        }

        internal static ITestHarness GetInMemoryTestHarness()
        {
            return InMemoryTestHarness.Create();
        }

        internal static ITestHarness GetSqlServerTestHarness(string testName)
        {
            var sqlServerDbSettings = "SqlServerDbSettings.json";
            /*
            Create this file in your DataStore.Tests project root and set it's build output to "copy always"
            This file should always be .gitignore(d), don't want to expose this in your repo.
            {
                "ConnectionString": "<connstring>"
            }
            */
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqlServerDbSettings);

            var sqlServerSettings = JsonConvert.DeserializeObject<SqlServerDbSettings>(File.ReadAllText(location));
            sqlServerSettings.TableName = testName;

            return SqlServerTestHarness.Create(sqlServerSettings);
        }
    }
}