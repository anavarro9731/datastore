namespace DataStore.Impl.SqlServer
{
    using System.Data.SqlClient;
    using DataStore.Models.PureFunctions.Extensions;

    public class SqlServerDbClientFactory
    {
        private readonly SqlServerDbSettings config;

        public SqlServerDbClientFactory(SqlServerDbSettings config)
        {
            this.config = config;
        }

        public SqlConnection OpenClient()
        {
            return new SqlConnection(this.config.ToConnectionString()).Op(c => c.Open());
        }
    }
}