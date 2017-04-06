using Newtonsoft.Json;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbSettings
    {
        [JsonConstructor]
        private SqlServerDbSettings(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public static SqlServerDbSettings Create(string connString)
        {
            return new SqlServerDbSettings(connString);
        }

        public const string SqlServerAggregatesTableName = "DataStoreAggregates";
    }
}