namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class InMemoryTestHarness : ITestHarness
    {
        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private InMemoryTestHarness(DataStoreOptions dataStoreOptions)
        {
            DocumentRepository = new InMemoryDocumentRepository();
            DataStore = new DataStore(DocumentRepository, this.messageAggregator, dataStoreOptions);
        }

        public IDataStore DataStore { get; }

        private InMemoryDocumentRepository DocumentRepository { get; }

        public static ITestHarness Create(DataStoreOptions dataStoreOptions = null)
        {
            return new InMemoryTestHarness(dataStoreOptions);
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //create a new one, we definately don't want to use the instance passed in, in the event it changes after this call
            //and affects the commit and/or the resulting events
            var clone = aggregate.Clone();
            
            //copied from datastore create capabilities, may get out of date
            DataStoreCreateCapabilities.ForceProperties(clone.ReadOnly, clone);

            clone.Created = DateTime.UtcNow.AddDays(-1);
            clone.CreatedAsMillisecondsEpochTime = DateTime.UtcNow.AddDays(-1).ConvertToSecondsEpochTime();
            clone.Modified = clone.Created;
            clone.ModifiedAsMillisecondsEpochTime = clone.ModifiedAsMillisecondsEpochTime;

            DocumentRepository.Aggregates.Add(clone);
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var queryResult = extendQueryable == null
                                  ? DocumentRepository.Aggregates.OfType<T>()
                                  : extendQueryable(DocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return queryResult;
        }
    }
}