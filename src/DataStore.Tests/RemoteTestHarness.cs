using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStore.DataAccess.Impl.DocumentDb;
using DataStore.DataAccess.Interfaces;
using DataStore.DataAccess.Interfaces.Events;
using DataStore.DataAccess.Models.Messages.Events;

namespace DataStore.Tests
{
    public class RemoteTestHarness : ITestHarness
    {
        public DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _eventAggregator.Events.OfType<IDataStoreEvent>().ToList();
        private DocumentRepository DocumentRepository { get; }
        private readonly EventAggregator _eventAggregator = new EventAggregator { PropogateDomainEvents = false, PropogateDataStoreEvents = true };

        public RemoteTestHarness(DocumentRepository documentRepository)
        {
            this.DocumentRepository = documentRepository;
            this.DataStore = new DataStore(this.DocumentRepository, _eventAggregator);
        }

        public async Task AddToDatabase<T>(T aggregate) where T: IAggregate
        {
            var newAggregate = new AggregateAdded<T>(nameof(AddToDatabase), aggregate, this.DocumentRepository);
            await newAggregate.CommitClosure();
        }

        public async Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T: IHaveSchema, IHaveAUniqueId
        {
            var query = extendQueryable == null ? this.DocumentRepository.CreateDocumentQuery<T>() : extendQueryable(this.DocumentRepository.CreateDocumentQuery<T>());
            return await this.DocumentRepository.ExecuteQuery<T>(new AggregatesQueried<T>(nameof(QueryDatabase), query.AsQueryable()));

        }
    }
}