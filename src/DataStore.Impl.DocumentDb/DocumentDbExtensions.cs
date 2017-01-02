namespace DataStore.Impl.DocumentDb
{
    using Microsoft.Azure.Documents.Client;
    using Models.Config;

    public static class DocumentDbExtensions
    {
        public static string DatabaseSelfLink(this DocumentDbSettings config)
        {
            return UriFactory.CreateDatabaseUri(config.DatabaseName).ToString();
        }

        public static string CollectionSelfLink(this DocumentDbSettings config)
        {
            return UriFactory.CreateDocumentCollectionUri(config.DatabaseName, config.CollectionSettings.CollectionName).ToString();
        }
    }
}