using System.Data.SqlClient;
using System.Text;
using System.Transactions;

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
                using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (var connection = factory.OpenClient())
                    {
                        using (var command = new SqlCommand(@"
                            IF NOT EXISTS ( 
                                SELECT * 
                                  FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_CATALOG = '" + settings.Database + @"' 
                                   AND TABLE_SCHEMA = 'dbo' 
                                   AND TABLE_NAME = '" + settings.TableName + @"'
                            )
                            BEGIN
                                CREATE TABLE " + settings.Database + @".dbo." + settings.TableName + @" (AggregateId uniqueidentifier, [Schema] nvarchar(250), Json nvarchar(max), Version rowversion)
                            END", connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    tx.Complete();
                }
            }
        }
    }
}