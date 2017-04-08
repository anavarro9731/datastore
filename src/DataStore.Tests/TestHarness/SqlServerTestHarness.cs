using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Impl.SqlServer;
using DataStore.Interfaces.Events;
using DataStore.Interfaces.LowLevel;
using DataStore.MessageAggregator;
using DataStore.Models.Messages.Events;
using DataStore.Models.PureFunctions.Extensions;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore.Tests.TestHarness
{
    public class SqlServerTestHarness : ITestHarness
    {
        private readonly IMessageAggregator eventAggregator = DataStoreMessageAggregator.Create();
        private readonly SqlServerRepository sqlServerRepository;

        private SqlServerTestHarness(SqlServerRepository sqlServerRepository)
        {
            this.sqlServerRepository = sqlServerRepository;
            DataStore = new DataStore(this.sqlServerRepository, eventAggregator);
        }

        public DataStore DataStore { get; }

        public List<IDataStoreEvent> Events => eventAggregator.AllMessages.OfType<IDataStoreEvent>().ToList();

        public async Task AddToDatabase<T>(T aggregate) where T : IAggregate
        {
            var newAggregate = new AggregateAdded<T>(nameof(AddToDatabase), aggregate, sqlServerRepository);
            await newAggregate.CommitClosure();
        }

        public async Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : IHaveSchema, IHaveAUniqueId
        {
            var query = extendQueryable == null
                ? sqlServerRepository.CreateDocumentQuery<T>()
                : extendQueryable(sqlServerRepository.CreateDocumentQuery<T>());
            return await sqlServerRepository.ExecuteQuery(new AggregatesQueried<T>(nameof(QueryDatabase), query.AsQueryable()));
        }

        public static ITestHarness Create(SqlServerDbSettings dbConfig)
        {
            ClearTestDatabase(dbConfig);

            return new SqlServerTestHarness(new SqlServerRepository(dbConfig));
        }

        private static void ClearTestDatabase(SqlServerDbSettings settings)
        {
            using (var client = new SqlServerDbClientFactory(settings).OpenClient())
            {
                client.CreateCommand().Op(c =>
                {
                    c.CommandText = "USE datastore; EXEC sp_msforeachtable 'DROP TABLE [?]'";
                    c.ExecuteNonQuery();
                });
            }
        }
    }
}