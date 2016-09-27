namespace DataStore.Infrastructure.Configuration.Settings
{
    public class FileStorageSettings
    {
        public FileStorageSettings(string connectionString, string storagePrefix)
        {
            this.ConnectionString = connectionString;
            this.StoragePrefix = storagePrefix;
        }

        public string ConnectionString { get; set; }

        public string StoragePrefix { get; set; }
    }
}