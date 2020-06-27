namespace DataStore.Tests.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        public static ITestHarness Create(DataStoreOptions dataStoreOptions = null) => new InMemoryTestHarness(dataStoreOptions);

        private InMemoryTestHarness(DataStoreOptions dataStoreOptions)
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore(DocumentRepository, this.messageAggregator, dataStoreOptions);
        }

        public IDataStore DataStore { get; }

        private InMemoryDocumentRepository DocumentRepository { get; }

        public void AddItemDirectlyToUnderlyingDb<T>(T aggregate) where T : class, IAggregate, new()
        {
            //create a new one, we definitely don't want to use the instance passed in, in the event it changes after this call
            //and affects the commit and/or the resulting events
            var clone = aggregate.Clone();

            DataStoreCreateCapabilities.ForceProperties(clone.ReadOnly, clone);

            DocumentRepository.Aggregates.Add(clone);
            clone.Etag = Guid.NewGuid().ToString(); //fake etag update internally
            aggregate.Etag = clone.Etag; //fake etag update externally
        }

        public List<T> QueryUnderlyingDbDirectly<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            var queryResult = extendQueryable == null
                                  ? DocumentRepository.Aggregates.OfType<T>()
                                  : extendQueryable(DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return queryResult.ToList();
        }
    }
}