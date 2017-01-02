namespace DataStore
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure.PureFunctions.Extensions;
    using Interfaces;
    using Interfaces.Addons;
    using Models.Messages.Events;

    internal class DataStoreCreateCapabilities : IDataStoreCreateCapabilities
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreCreateCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
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

            _eventAggregator.Store(new AggregateAdded<T>(nameof(Create), enriched, DsConnection));

            return Task.FromResult(enriched);
        }

        #endregion
    }
}