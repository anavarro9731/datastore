using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces.Events;
using DataStore.Interfaces.LowLevel;
using DataStore.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.Tests.TestHarness
{
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
        public List<IQueuedDataStoreWriteOperation> QueuedWriteOperations => messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().ToList();
        public List<IMessage> AllMessages => messageAggregator.AllMessages.ToList();

        public Task AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //copied from datastore create capabilities, may get out of date
            DataStoreCreateCapabilities.ForcePropertiesOnCreate(aggregate.ReadOnly, aggregate);

            DocumentRepository.Aggregates.Add(aggregate);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : IHaveSchema, IHaveAUniqueId
        {
            var queryResult = extendQueryable == null
                ? DocumentRepository.Aggregates.OfType<T>()
                : extendQueryable(DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return Task.FromResult(queryResult);
        }

        public static ITestHarness Create()
        {
            return new InMemoryTestHarness();
        }
    }
}