namespace DataStore.Providers.CosmosDb
{
    public class CosmosSettings
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
    }
}