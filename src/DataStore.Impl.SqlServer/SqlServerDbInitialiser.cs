using System.Data.SqlClient;

namespace DataStore.Impl.SqlServer
{
    public class SqlServerDbInitialiser
    {
        public static void Initialise(SqlServerDbClientFactory factory, SqlServerDbSettings settings)
        {
            {
                TryCreateTable();
            }

            void TryCreateTable()
            {
                using (var connection = factory.OpenClient())
                {
                    try
                    {
                        using (var command = new SqlCommand(
                            $"CREATE TABLE {settings.TableName} (AggregateId uniqueidentifier, [Schema] nvarchar(250), Json nvarchar(max))", connection))
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