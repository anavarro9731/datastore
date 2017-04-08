using Newtonsoft.Json;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbSettings
    {
        public const string SqlServerAggregatesTableName = "DataStoreAggregates";

        [JsonConstructor]
        private SqlServerDbSettings(string serverInstance, string database, string userId, string password)
        {
            ServerInstance = serverInstance;
            Database = database;
            UserId = userId;
            Password = password;
        }

        public string ServerInstance { get; }
        public string Database { get; }
        public string UserId { get; }
        public string Password { get; }

        public string ToConnectionString()
        {
            return $"Server={ServerInstance};Database={Database};User Id={UserId};Password={Password}";
        }
    }
}