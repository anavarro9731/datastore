using Newtonsoft.Json;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbSettings
    {
        public string ServerInstance { get; }
        public string Database { get; }
        public string UserId { get; }
        public string Password { get; }

        [JsonConstructor]
        private SqlServerDbSettings(string serverInstance, string database, string userId, string password)
        {
            ServerInstance = serverInstance;
            Database = database;
            UserId = userId;
            Password = password;
        }

        pu

        public const string SqlServerAggregatesTableName = "DataStoreAggregates";
    }
}