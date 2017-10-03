namespace DataStore.Impl.SqlServer
{
    using Newtonsoft.Json;

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

        public string Database { get; }

        public string Password { get; }

        public string ServerInstance { get; }

        public string TableName { get; set; }

        public string UserId { get; }

        public static SqlServerDbSettings Create(string serverInstance, string database, string userId, string password, string tableName)
        {
            return new SqlServerDbSettings(serverInstance, database, userId, password, tableName);
        }

        public string ToConnectionString()
        {
            return $"Server={ServerInstance};Database={Database};User Id={UserId};Password={Password}";
        }
    }
}