namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;

    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private InMemoryTestHarness()
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore(DocumentRepository, this.messageAggregator);
        }

        public List<IMessage> AllMessages => this.messageAggregator.AllMessages.ToList();

        public DataStore DataStore { get; }

        private InMemoryDocumentRepository DocumentRepository { get; }

        public static ITestHarness Create()
        {
            return new InMemoryTestHarness();
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //copied from datastore create capabilities, may get out of date
            DataStoreCreateCapabilities.ForceProperties(aggregate.ReadOnly, aggregate);

            DocumentRepository.Aggregates.Add(aggregate);
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var queryResult = extendQueryable == null
                                  ? DocumentRepository.Aggregates.OfType<T>()
                                  : extendQueryable(DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return queryResult;
        }
    }
}