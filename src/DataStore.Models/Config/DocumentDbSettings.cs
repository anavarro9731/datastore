namespace DataStore.Models.Config
{
    public class DocumentDbSettings
    {
        public DocumentDbSettings(
            string authorizationKey,
            string databaseName,
            DocDbCollectionSettings collectionSettings,
            string endpointUrl)
        {
            AuthorizationKey = authorizationKey;
            DatabaseName = databaseName;
            CollectionSettings = collectionSettings;
            EndpointUrl = endpointUrl;
        }

        public string AuthorizationKey { get; }

        public string DatabaseName { get; }

        public DocDbCollectionSettings CollectionSettings { get; set; }

        public string EndpointUrl { get; }

    }
}