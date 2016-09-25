namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;
    using DataAccess.Messages.Events;
    using Infrastructure.Interfaces;
    using Infrastructure.PureFunctions.Extensions;

    internal class DataStoreCreateCapabilities : IDataStoreCreateCapabilities
    {
        private readonly IEventAggregator eventAggregator;

        public DataStoreCreateCapabilities(IDocumentRepository dataStoreConnection, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> Create<T>(T model, bool readOnly = false, bool hidden = false) where T : IAggregate, new()
        {
            var enriched = new T();
            enriched.UpdateFromAnotherObject(model);
            enriched.Op(
                e =>
                    {
                        e.Active = true;
                        e.Hidden = hidden;
                        e.ReadOnly = readOnly;

                        e.Created = DateTime.UtcNow;
                        e.Modified = DateTime.UtcNow;

                        e.id = model.id == default(Guid) ? Guid.NewGuid() : model.id;

                        e.SetScope((model.ScopeObjectIds ?? new List<Guid>()).ToArray());
                    });
            enriched.WalkGraphAndUpdateEntityMeta();

            return await eventAggregator.Store(new AggregateAdded<T>(enriched)).ForwardToAsync(DsConnection.AddAsync);
        }
    }
}