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

        public string AuthKey { get; set; }

        public string DatabaseName { get; set; }

        public string EndpointUrl { get; set; }

        public IDocumentRepository CreateRepository()
        {
            return new CosmosDbRepository(this);
        }
    }
}