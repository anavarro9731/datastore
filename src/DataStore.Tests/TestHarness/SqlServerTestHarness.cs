using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Impl.SqlServer;
using DataStore.Interfaces.Events;
using DataStore.Interfaces.LowLevel;
using DataStore.MessageAggregator;
using DataStore.Models.Messages;
using DataStore.Models.PureFunctions.Extensions;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Tests.TestHarness
{
    public class SqlServerTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();
        private readonly SqlServerRepository sqlServerRepository;

        private SqlServerTestHarness(SqlServerRepository sqlServerRepository)
        {
            this.sqlServerRepository = sqlServerRepository;
            DataStore = new DataStore(this.sqlServerRepository, messageAggregator);
        }

        public DataStore DataStore { get; }

        public List<IMessage> AllMessages => messageAggregator.AllMessages.ToList();

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate, sqlServerRepository, messageAggregator);
            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            var query = extendQueryable == null
                ? sqlServerRepository.CreateDocumentQuery<T>()
                : extendQueryable(sqlServerRepository.CreateDocumentQuery<T>());
            return sqlServerRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
        }

        public static ITestHarness Create(SqlServerDbSettings dbConfig)
        {
            ClearTestDatabase(dbConfig);

            return new SqlServerTestHarness(new SqlServerRepository(dbConfig));
        }

        private static void ClearTestDatabase(SqlServerDbSettings settings)
        {
            DropExistingAggregatesTable();

            SqlServerDbInitialiser.Initialise(new SqlServerDbClientFactory(settings), settings);

            void DropExistingAggregatesTable()
            {
                using (var client = new SqlServerDbClientFactory(settings).OpenClient())
                {
                    try
                    {
                        client.CreateCommand()
                            .Op(c =>
                            {
                                c.CommandText = $"DROP TABLE {settings.TableName};";
                                c.ExecuteNonQuery();
                            });
                    }
                    catch
                    {
                        // ignored, may not exist
                    }
                }
            }
        }       
    }
}