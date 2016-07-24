namespace Finygo.DocumentDb
{
    using Infrastructure.Configuration.Settings;

    using Microsoft.Azure.Documents.Client;

    public static class DocumentDbExtensions
    {
        public static string DatabaseSelfLink(this DocumentDbSettings config)
        {
            return UriFactory.CreateDatabaseUri(config.DatabaseName).ToString();
        }
    }
}