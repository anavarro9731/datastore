using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DataStore.Tests")]

namespace DataStore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.LowLevel;
    using Models.Messages.Events;
    using Models.PureFunctions;
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

            UpdateFromAnotherObject(model, enriched);
            
            ForcePropertiesOnCreate(readOnly, enriched);

            messageAggregator.Collect(new AggregateAdded<T>(nameof(Create), enriched, DsConnection));

            return Task.FromResult(enriched);
        }

        internal static void ForcePropertiesOnCreate<T>(bool readOnly, T enriched) where T : IAggregate
        {
            enriched.Op(
                e =>
                {
                    //aggregate
                    e.ReadOnly = readOnly;
                    e.ScopeReferences = e.ScopeReferences ?? new List<IScopeReference>();
                });

            WalkGraphAndUpdateEntityMeta(enriched);
        }

        public void UpdateFromAnotherObject<T>(T source, T destination)
        {
            Guard.Against(!source.GetType().InheritsOrImplements(destination.GetType()),
                "Source object not of the same base type");

            source.CopyProperties(destination);
        }

        private static void WalkGraphAndUpdateEntityMeta(object current)
        {
            if (current != null)
            {
                var t = current.GetType();

                foreach (var p in t.GetProperties())
                    if (p.Name == nameof(IEntity.id))
                    {
                        //set an id for any entity in the tree if it doesn't have one
                        //regardless of whether it is the aggregate or a child entity
                        //in many cases this will already be done in the app code
                        if ((Guid) p.GetValue(current, null) == Guid.Empty)
                            p.SetValue(current, Guid.NewGuid(), null);
                    }
                    else if (p.Name == nameof(IEntity.Created))
                    {
                        //set created datetime if this is null
                        if ((DateTime?) p.GetValue(current, null) == null)
                            p.SetValue(current, DateTime.UtcNow, null);
                    }
                    else if (p.Name == nameof(IEntity.CreatedAsMillisecondsEpochTime))
                    {
                        //set created datetime if this is null
                        if (p.GetValue(current, null) == null)
                            p.SetValue(current, DateTime.UtcNow.ConvertToSecondsEpochTime(), null);
                    }
                    else if (p.Name == nameof(IEntity.Modified))
                    {
                        //set modified if this is the root model
                        if (current is Aggregate)
                            p.SetValue(current, DateTime.UtcNow, null);
                    }
                    else if (p.Name == nameof(IEntity.ModifiedAsMillisecondsEpochTime))
                    {
                        //set modified if this is the root model
                        if (current is Aggregate)
                            p.SetValue(current, DateTime.UtcNow.ConvertToSecondsEpochTime(), null);
                    }
                    else if (!p.PropertyType.IsSystemType())
                    {
                        //one-to-one reference                    
                        WalkGraphAndUpdateEntityMeta(p.GetValue(current, null));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    {
                        var collection = p.GetValue(current, null);
                        if (collection != null)
                            foreach (var sub in (IEnumerable) p.GetValue(current, null))
                                WalkGraphAndUpdateEntityMeta(sub);
                    }
            }
        }

        #endregion
    }
}