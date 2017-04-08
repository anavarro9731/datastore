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
            return GetSqlServerTestHarness();
            //return GetInMemoryTestHarness();
        }

        private static ITestHarness GetDocumentDbTestHarness(string testName)
        {
            var options = TestHarnessOptions.Create(DocDbCollectionSettings.Create(testName));

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

        public static ITestHarness GetInMemoryTestHarness()
        {
            return InMemoryTestHarness.Create();
        }

        public static ITestHarness GetSqlServerTestHarness()
        {

            var documentdbsettingsJson = "SqlServerDbSettings.json";
            /*
            Create this file in your DataStore.Tests project root and set it's build output to "copy always"
            This file should always be .gitignore(d), don't want to expose this in your repo.
            {
                "ConnectionString": "<connstring>"
            }
            */
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, documentdbsettingsJson);

            var sqlServerSettings = JsonConvert.DeserializeObject<SqlServerDbSettings>(File.ReadAllText(location));

            return SqlServerTestHarness.Create(sqlServerSettings);
        }
    }
}