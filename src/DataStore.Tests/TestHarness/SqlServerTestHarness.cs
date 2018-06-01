namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using global::DataStore.Impl.SqlServer;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class SqlServerTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private readonly SqlServerRepository sqlServerRepository;

        private SqlServerTestHarness(SqlServerRepository sqlServerRepository, DataStoreOptions dataStoreOptions)
        {
            this.sqlServerRepository = sqlServerRepository;
            DataStore = new DataStore(this.sqlServerRepository, this.messageAggregator, dataStoreOptions);
        }

        public List<IMessage> AllMessages => this.messageAggregator.AllMessages.ToList();

        public DataStore DataStore { get; }

        public static ITestHarness Create(SqlServerDbSettings dbConfig, DataStoreOptions dataStoreOptions)
        {
            ClearTestDatabase(dbConfig);

            return new SqlServerTestHarness(new SqlServerRepository(dbConfig), dataStoreOptions);
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate, this.sqlServerRepository, this.messageAggregator);
            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = extendQueryable == null
                            ? this.sqlServerRepository.CreateDocumentQuery<T>()
                            : extendQueryable(this.sqlServerRepository.CreateDocumentQuery<T>());
            return this.sqlServerRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
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
                        client.CreateCommand().Op(
                            c =>
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