namespace DataStore
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Models.Messages.Events;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    //methods return the enriched object as it was added to the database

    internal class DataStoreCreateCapabilities : IDataStoreCreateCapabilities
    {
        private readonly IMessageAggregator messageAggregator;

        public DataStoreCreateCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        #region IDataStoreCreateCapabilities Members

        public Task<T> Create<T>(T model, bool readOnly = false) where T : IAggregate, new()
        {
            var enriched = new T();
            enriched.UpdateFromAnotherObject(model);
            enriched.Op(
                e =>
                {
                    e.Active = true;
                    e.ReadOnly = readOnly;

                    e.Created = DateTime.UtcNow;
                    e.Modified = DateTime.UtcNow;

                    e.id = model.id == default(Guid) ? Guid.NewGuid() : model.id;

                    e.ScopeReferences = model.ScopeReferences;
                });

            enriched.WalkGraphAndUpdateEntityMeta();

            messageAggregator.Collect(new AggregateAdded<T>(nameof(Create), enriched, DsConnection));

            return Task.FromResult(enriched);
        }

        #endregion
    }
}