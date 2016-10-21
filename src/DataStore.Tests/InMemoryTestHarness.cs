using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.DataAccess.Impl.DocumentDb;
using DataStore.DataAccess.Interfaces;
using DataStore.DataAccess.Interfaces.Events;

namespace DataStore.Tests
{
    public class InMemoryTestHarness : ITestHarness
    {
        public DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _eventAggregator.Events.OfType<IDataStoreEvent>().ToList();
        private InMemoryDocumentRepository  DocumentRepository { get; }
        private readonly EventAggregator _eventAggregator = new EventAggregator { PropogateDomainEvents = false, PropogateDataStoreEvents = true };

        public InMemoryTestHarness()
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
    }
}