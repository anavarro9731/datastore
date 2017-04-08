using System.Data.SqlClient;
using DataStore.Models.PureFunctions.Extensions;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbClientFactory
    {
        private readonly SqlServerDbSettings config;

        public SqlServerDbClientFactory(SqlServerDbSettings config)
        {
            this.config = config;            
        }
            
        public SqlConnection OpenClient()
        {
            return new SqlConnection(config.ConnectionString).Op(c => c.Open());
        }
    }

    
}