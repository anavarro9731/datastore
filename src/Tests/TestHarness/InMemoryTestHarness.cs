using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore;
using DataStore.Impl.DocumentDb;
using DataStore.Interfaces;
using DataStore.Interfaces.Events;

namespace Tests.TestHarness
{
    public class InMemoryTestHarness : ITestHarness
    {
        public DataStore.DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _eventAggregator.Events.OfType<IDataStoreEvent>().ToList();
        private InMemoryDocumentRepository  DocumentRepository { get; }
        private readonly IEventAggregator _eventAggregator = EventAggregator.Create(false);

        private InMemoryTestHarness()
        {
            this.DocumentRepository = new InMemoryDocumentRepository();
            this.DataStore = new DataStore.DataStore(this.DocumentRepository, _eventAggregator);
        }
        
        public Task AddToDatabase<T>(T aggregate) where T: IAggregate
        {
            this.DocumentRepository.Aggregates.Add(aggregate);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : IHaveSchema, IHaveAUniqueId
        {
            var queryResult = extendQueryable == null ? this.DocumentRepository.Aggregates.OfType<T>() : extendQueryable(this.DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return Task.FromResult(queryResult);
        }

        public static ITestHarness Create()
        {
            return new InMemoryTestHarness();
        }
    }
}