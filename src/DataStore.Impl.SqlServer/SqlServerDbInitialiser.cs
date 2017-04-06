using System.Data.SqlClient;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbInitialiser
    {
        public static void Initialise(SqlServerDbClientFactory factory)
        {
            {
                TryCreateTable();
            }

            void TryCreateTable()
            {
                using (var connection = factory.GetClient())
                {
                    connection.Open();
                    try
                    {
                        using (var command = new SqlCommand(
                            "CREATE TABLE DataStoreAggregates (AggregateId uniqueidentifier, Schema nvarchar(250), Json nvarchar(max))", connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        // ignored
                        // wiil not work if table already exists
                    }
                }
            }
        }
    }
}