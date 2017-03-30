namespace DataStore.Impl.DocumentDb.Config
{
    public class DocumentDbSettings
    {
        private DocumentDbSettings(
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

        public static DocumentDbSettings Create(
            string authorizationKey,
            string databaseName,
            DocDbCollectionSettings collectionSettings,
            string endpointUrl)
        {
            return new DocumentDbSettings(authorizationKey, databaseName, collectionSettings, endpointUrl);
        }
    }
}