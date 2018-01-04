namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using global::DataStore.Impl.RavenDb;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages;

    public class RavenTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private readonly RavenRepository ravenRepository;

        private RavenTestHarness(RavenRepository ravenRepository)
        {
            this.ravenRepository = ravenRepository;
            DataStore = new DataStore(this.ravenRepository, this.messageAggregator);
        }

        public List<IMessage> AllMessages => this.messageAggregator.AllMessages.ToList();

        public DataStore DataStore { get; }

        public static ITestHarness Create(RavenSettings dbConfig)
        {
            RavenRepository ravenRepository = new RavenRepository(dbConfig);
            ravenRepository.DropDatabase(dbConfig.Database);

            return new RavenTestHarness(ravenRepository);
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate, this.ravenRepository, this.messageAggregator);
            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = extendQueryable == null
                            ? this.ravenRepository.CreateDocumentQuery<T>()
                            : extendQueryable(this.ravenRepository.CreateDocumentQuery<T>());
            return this.ravenRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
        }
    }
}