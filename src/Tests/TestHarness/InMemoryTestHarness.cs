using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.Impl.DocumentDb;
using DataStore.Interfaces;
using DataStore.Interfaces.Events;
using PalmTree.Infrastructure.EventAggregator;
using PalmTree.Infrastructure.Interfaces;

namespace Tests.TestHarness
{
    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IEventAggregator eventAggregator = EventAggregator.Create();

        private InMemoryTestHarness()
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore.DataStore(DocumentRepository, eventAggregator);
        }

        private InMemoryDocumentRepository DocumentRepository { get; }
        public DataStore.DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => eventAggregator.Events.OfType<IDataStoreEvent>().ToList();

        public Task AddToDatabase<T>(T aggregate) where T : IAggregate
        {
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