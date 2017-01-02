using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataStore.Tests.TestHarness
{
    using Impl.DocumentDb;
    using Interfaces;
    using Interfaces.Addons;
    using Interfaces.Events;

    public class InMemoryTestHarness : ITestHarness
    {
        public DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _eventAggregator.Events.OfType<IDataStoreEvent>().ToList();
        private InMemoryDocumentRepository  DocumentRepository { get; }
        private readonly IEventAggregator _eventAggregator = EventAggregator.Create(false);

        private InMemoryTestHarness()
        {
            this.DocumentRepository = new InMemoryDocumentRepository();
            this.DataStore = new DataStore(this.DocumentRepository, _eventAggregator);
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