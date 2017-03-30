namespace DataStore.Impl.DocumentDb.Config
{
    public class FileStorageSettings
    {
        private FileStorageSettings(string connectionString, string storagePrefix)
        {
            ConnectionString = connectionString;
            StoragePrefix = storagePrefix;
        }

        public string ConnectionString { get; }

        public string StoragePrefix { get; }

        public static FileStorageSettings Create(string connectionString, string storagePrefix)
        {
            return new FileStorageSettings(connectionString, storagePrefix);
        }
    }
}