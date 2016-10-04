namespace DataStore.DataAccess.Impl.DocumentDb
{
    using Microsoft.Azure.Documents.Client;
    using Models.Config;

    public static class DocumentDbExtensions
    {
        public static string DatabaseSelfLink(this DocumentDbSettings config)
        {
            return UriFactory.CreateDatabaseUri(config.DatabaseName).ToString();
        }
    }
}