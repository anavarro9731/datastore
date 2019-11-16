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

        public string AuthKey { get; private set; }

        public string DatabaseName { get; private set; }

        public string EndpointUrl { get; private set; }

        public int MaxQueryCostInRus { get; set; } = 400;

        public int MaxItemsPerBatchServerLimit => 1000;

        public int MaxItemsPerBatchClientDefault => 100;

        public IDocumentRepository CreateRepository()
        {
            return new CosmosDbRepository(this);
        }
    }
}