namespace DataStore.Providers.CosmosDb
{
    using DataStore.Interfaces;

    public class CosmosSettings : IDatabaseSettings
    {
        public CosmosSettings(string authKey, string databaseName, string endpointUrl)
        {
            AuthKey = authKey;
            DatabaseName = databaseName;
            EndpointUrl = endpointUrl;
        }

        public string AuthKey { get; }

        public string DatabaseName { get; }

        public string EndpointUrl { get; }

        public int MaxItemsPerBatchClientDefault => 100;

        public int MaxItemsPerBatchServerLimit => 1000;

        public int MaxQueryCostInRus { get; set; } = 400;

        public IDocumentRepository CreateRepository() => new CosmosDbRepository(this);
    }
}
