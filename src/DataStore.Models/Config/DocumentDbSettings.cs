namespace DataStore.DataAccess.Models.Config
{
    public class DocumentDbSettings
    {
        public DocumentDbSettings(
            string authorizationKey, 
            string databaseName, 
            string defaultCollectionName, 
            string endpointUrl)
        {
            this.AuthorizationKey = authorizationKey;
            this.DatabaseName = databaseName;
            this.DefaultCollectionName = defaultCollectionName;
            this.EndpointUrl = endpointUrl;
        }

        public string AuthorizationKey { get; }

        public string DatabaseName { get; }

        public string DefaultCollectionName { get; }

        public string EndpointUrl { get; }
    }
}