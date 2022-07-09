namespace DataStore.Tests.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        public static ITestHarness Create(bool useHierarchicalPartitionKeys, DataStoreOptions dataStoreOptions = null) => new InMemoryTestHarness(dataStoreOptions, useHierarchicalPartitionKeys);

        private InMemoryTestHarness(DataStoreOptions dataStoreOptions, bool useHierarchicalPartitionKeys)
        {
            DocumentRepository = new InMemoryDocumentRepository(useHierarchicalPartitionKeys);
            DataStore = new DataStore(DocumentRepository, this.messageAggregator, dataStoreOptions);
        }

        public IDataStore DataStore { get; }

        private InMemoryDocumentRepository DocumentRepository { get; }

        public void AddItemDirectlyToUnderlyingDb<T>(T aggregate) where T : class, IAggregate, new()
        {
            /* create a new one, we definitely don't want to use the instance passed in, in the event it changes after this call
            and affects the commit and/or the resulting events */
            var clone = aggregate.Clone();

            //* get the value from when the test harness was setup and make sure to stay in-sync with that
            var useHierarchicalPartitionKeys = DataStore.DocumentRepository.UseHierarchicalPartitionKeys;
            clone.ForcefullySetMandatoryPropertyValues(clone.ReadOnly, useHierarchicalPartitionKeys);

            //* add it as close to the metal as you can without having to specify the partition key directly
            DocumentRepository.AddAsync(new CreateOperation<T>()
            {
                TypeName = typeof(T).FullName,
                Created = DateTime.UtcNow,
                Model = aggregate
            });
            clone.Etag = Guid.NewGuid().ToString(); //fake etag update internally
            aggregate.Etag = clone.Etag; //fake etag update externally
        }

        public List<T> QueryUnderlyingDbDirectly<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            //* fan out means you don't have to provide a partition key
            var fanoutAcrossAllPartitions = DocumentRepository.AggregatesByLogicalPartition.Values.SelectMany(x => x);
            
            var queryResult = extendQueryable == null
                                  ? fanoutAcrossAllPartitions.OfType<T>()
                                  : extendQueryable(fanoutAcrossAllPartitions.OfType<T>().AsQueryable());
            return queryResult.ToList();
        }
    }
}