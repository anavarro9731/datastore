namespace DataStore.Impl.DocumentDb
{
    using Config;
    using Microsoft.Azure.Documents.Client;

    public static class DocumentDbExtensions
    {
        public static string DatabaseSelfLink(this DocumentDbSettings config)
        {
            return UriFactory.CreateDatabaseUri(config.DatabaseName).ToString();
        }

        public static string CollectionSelfLink(this DocumentDbSettings config)
        {
            return
                UriFactory.CreateDocumentCollectionUri(config.DatabaseName, config.CollectionSettings.CollectionName).ToString();
        }
    }
}