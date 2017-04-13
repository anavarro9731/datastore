using Newtonsoft.Json;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbSettings
    {
        [JsonConstructor]
        private SqlServerDbSettings(string serverInstance, string database, string userId, string password, string tableName)
        {
            ServerInstance = serverInstance;
            Database = database;
            UserId = userId;
            Password = password;
            TableName = tableName ?? "DataStoreAggregates";
        }

        public string ServerInstance { get; }
        public string Database { get; }
        public string UserId { get; }
        public string Password { get; }
        public string TableName { get; set; }

        public static SqlServerDbSettings Create(string serverInstance, string database, string userId, string password,
            string tableName)
        {
            return new SqlServerDbSettings
            (
                serverInstance,
                database,
                userId,
                password,
                tableName
            );
        }

        public string ToConnectionString()
        {
            return $"Server={ServerInstance};Database={Database};User Id={UserId};Password={Password}";
        }
    }
}