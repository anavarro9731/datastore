namespace DataStore.Tests.TestHarness
{
    using System;
    using System.IO;
    //using global::DataStore.Impl.DocumentDb.Config;
    //using global::DataStore.Impl.SqlServer;
    using Newtonsoft.Json;

    public static class TestHarness
    {
        public static ITestHarness Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            //return GetDocumentDbTestHarness(testName, dataStoreOptions: dataStoreOptions);
            return GetInMemoryTestHarness(dataStoreOptions);
        }

        //internal static ITestHarness GetDocumentDbTestHarness(string testName, DocDbCollectionSettings collectionSettings = null, DataStoreOptions dataStoreOptions = null)
        //{
        //    var options = TestHarnessOptions.Create(collectionSettings ?? DocDbCollectionSettings.Create(testName));

        //    var documentdbsettingsJson = "DocumentDbSettings.json";
        //    /*
        //    Create this file in your DataStore.Tests project root and set it's build output to "copy always"
        //    This file should always be .gitignore(d), don't want to expose this in your repo.
        //    {
        //        "AuthorizationKey": "<azurekey>",
        //        "DatabaseName": "<dbname>",
        //        "EndpointUrl": "<endpointurl>"
        //    }
        //    */
        //    var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, documentdbsettingsJson);

        //    var documentDbSettings = JsonConvert.DeserializeObject<DocumentDbSettings>(File.ReadAllText(location));

        //    documentDbSettings.CollectionSettings = options.CollectionSettings;

        //    return DocumentDbTestHarness.Create(documentDbSettings, dataStoreOptions);
        //}

        internal static ITestHarness GetInMemoryTestHarness(DataStoreOptions dataStoreOptions = null)
        {
            return InMemoryTestHarness.Create(dataStoreOptions);
        }
    }
}