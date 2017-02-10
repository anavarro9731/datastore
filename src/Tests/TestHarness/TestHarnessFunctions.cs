using System;
using System.IO;
using DataStore.Models.Config;
using Newtonsoft.Json;

namespace Tests.TestHarness
{
    public static class TestHarnessFunctions
    {
        public static ITestHarness GetTestHarness(string testName)
        {
            return GetTestHarness(TestHarnessOptions.Create(DocDbCollectionSettings.Create(
                testName)));
        }

        public static ITestHarness GetTestHarness(TestHarnessOptions options)
        {
            if (options.CollectionSettings.CollectionName.StartsWith("Integration"))
            {
                var documentdbsettingsJson = "DocumentDbSettings.json";
                /*
                Create this file in your DataStore.Tests project root and set it's build output to "copy always"
                {
                    "AuthorizationKey": "<azurekey>",
                    "DatabaseName": "<dbname>",
                    "EndpointUrl": "<endpointurl>"
                }
                */
                var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, documentdbsettingsJson);

                var documentDbSettings = JsonConvert.DeserializeObject<DocumentDbSettings>(File.ReadAllText(location));

                documentDbSettings.CollectionSettings = options.CollectionSettings;

                return RemoteTestHarness.Create(documentDbSettings);
            }

            return InMemoryTestHarness.Create();
        }
    }
}