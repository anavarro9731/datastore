using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.LowLevel;
using DataStore.Models.Messages;
using DataStore.Models.PureFunctions.Extensions;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

[assembly: InternalsVisibleTo("DataStore.Tests")]

namespace DataStore
{
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

        #region

        public Task<T> Create<T>(T model, bool readOnly = false) where T : class, IAggregate, new()
        {
            //create a new one, we definately don't want to use the instance passed in, in the event it changes after this call
            //and affects the commit and/or the resulting events
            var newObject = model.Clone();

            ForceProperties(readOnly, newObject);

            messageAggregator.Collect(new QueuedCreateOperation<T>(nameof(Create), newObject, DsConnection, messageAggregator));

            //for the same reason as the above we want a new object, but we want to return the enriched one, so we clone it,
            //essentially no external client should be able to get a reference to the instance we use internally
            return Task.FromResult(newObject.Clone());
        }

        #endregion

        internal static void ForceProperties<T>(bool readOnly, T enriched) where T : class, IAggregate, new()
        {
            enriched.Op(
                e =>
                {
                    //aggregate
                    e.schema = typeof(T).FullName; //should be defaulted by Aggregate but needs to be forced
                    e.ReadOnly = readOnly;
                    e.ScopeReferences = e.ScopeReferences ?? new List<IScopeReference>();
                });

            WalkGraphAndUpdateEntityMeta(enriched);
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
    }
}