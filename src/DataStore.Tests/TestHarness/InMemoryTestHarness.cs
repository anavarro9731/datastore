namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private InMemoryTestHarness()
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore(DocumentRepository, messageAggregator);
        }

        private InMemoryDocumentRepository DocumentRepository { get; }
        public DataStore DataStore { get; }

        public List<IDataStoreOperation> Operations => messageAggregator.AllMessages.OfType<IDataStoreOperation>().ToList();

        public List<IQueuedDataStoreWriteOperation> QueuedWriteOperations => messageAggregator.AllMessages
            .OfType<IQueuedDataStoreWriteOperation>()
            .ToList();

        public List<IMessage> AllMessages => messageAggregator.AllMessages.ToList();

        #region

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //copied from datastore create capabilities, may get out of date
            DataStoreCreateCapabilities.ForceProperties(aggregate.ReadOnly, aggregate);

            DocumentRepository.Aggregates.Add(aggregate);
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            var queryResult = extendQueryable == null
                ? DocumentRepository.Aggregates.OfType<T>()
                : extendQueryable(DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return queryResult;
        }

        #endregion

        public static ITestHarness Create()
        {
            return new InMemoryTestHarness();
        }
    }
}