using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Interfaces.Events;
using DataStore.Interfaces.LowLevel;
using DataStore.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore.Tests.TestHarness
{
    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator _messageAggregator = DataStoreMessageAggregator.Create();

        private InMemoryTestHarness()
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore(DocumentRepository, _messageAggregator);
        }

        private InMemoryDocumentRepository DocumentRepository { get; }
        public DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _messageAggregator.AllMessages.OfType<IDataStoreEvent>().ToList();

        public Task AddToDatabase<T>(T aggregate) where T : IAggregate
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