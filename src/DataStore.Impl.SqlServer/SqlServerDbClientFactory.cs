using System.Data.SqlClient;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbClientFactory
    {
        private readonly SqlServerDbSettings config;

        public SqlServerDbClientFactory(SqlServerDbSettings config)
        {
            this.config = config;            
        }
            
        public SqlConnection GetClient()
        {
            return new SqlConnection(config.ConnectionString);
        }
    }

    
}