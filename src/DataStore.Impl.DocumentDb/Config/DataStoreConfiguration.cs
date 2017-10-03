namespace DataStore.Impl.DocumentDb.Config
{
    /* 
    Because what goes in this class is effectively global, it is important that it stays limited to 2 things:
    1. Data required to access other ley data sources whether for read or write (e.g. database connection strings, file paths, etc.
    2. Data requuired for setup of key infrastructure for which it improves the conceptual clarity of the application to see it made explcit
    (e.g. identity server configuration properties, etc)
    
    What should not go in this class is:
    1. Anything mutatable, including on child objects.
    2. SENSITIVE Information, this should always be stored in environment variables, so it is never included in source control.

    Because the class is immutable you only configure it once, so there is not chance of changing the root configuration later.
    */

    public class DataStoreConfiguration
    {
        private DataStoreConfiguration(DocumentDbSettings documentDbSettings, FileStorageSettings fileStorageSettings)
        {
            DocumentDbSettings = documentDbSettings;
            FileStorageSettings = fileStorageSettings;
        }

        public DocumentDbSettings DocumentDbSettings { get; }

        public FileStorageSettings FileStorageSettings { get; }

        public static DataStoreConfiguration Create(DocumentDbSettings documentDbSettings, FileStorageSettings fileStorageSettings = null)
        {
            return new DataStoreConfiguration(documentDbSettings, fileStorageSettings);
        }
    }
}