﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DataStore.Tests")]

namespace DataStore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    //methods return the enriched object as it was added to the database

    internal class DataStoreCreateCapabilities : IDataStoreCreateCapabilities
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly IncrementVersions incrementVersions;

        public DataStoreCreateCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator, IncrementVersions incrementVersions)
        {
            this.messageAggregator = messageAggregator;
            this.incrementVersions = incrementVersions;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> Create<T>(T model, bool readOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            //create a new one, we definitely don't want to use the instance passed in, in the event it changes after this call
            //and affects the commit and/or the resulting events
            var newObject = model.Clone();

            ForceProperties(readOnly, newObject);

            Guard.Against(this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>().SingleOrDefault(e => !e.Committed && e.AggregateId == newObject.id)
                              != null, $"An item with the same ID of {newObject.id} is already queued to be created", Guid.Parse("63328bcd-d58d-446a-bc85-fedfde43d2e2"));

            int count = (await this.DsConnection.CountAsync(new AggregateCountedOperation<T>(methodName, t => t.id == newObject.id)).ConfigureAwait(false));

            bool existsAlready = count > 0;
            Guard.Against(existsAlready, $"An item with the same ID of {newObject.id} already exists", Guid.Parse("cfe3ebc2-4677-432b-9ded-0ef498b9f59d"));

            var clone = newObject.Clone();
            clone.Etag = "waiting to be committed";

            void EtagUpdated(string newTag) => clone.Etag = newTag;

            this.messageAggregator.Collect(new QueuedCreateOperation<T>(methodName, newObject, DsConnection, this.messageAggregator, EtagUpdated));

            await this.incrementVersions.IncrementAggregateVersionOfQueuedItem(newObject, methodName);

            //for the same reason as the above we want a new object, but we want to return the enriched one, so we clone it,
            //essentially no external client should be able to get a reference to the instance we use internally
            return clone;
        }

        internal static void ForceProperties<T>(bool readOnly, T newObject) where T : class, IAggregate, new()
        {
            newObject.Schema = typeof(T).FullName; //will be defaulted by Aggregate but needs to be forced as it is open to change because of serialisation opening the property setter
            newObject.ReadOnly = readOnly;
            newObject.VersionHistory = new List<Aggregate.AggregateVersionInfo>(); //-set again in commitchanges, still best not to allow any invalid state

            WalkGraphAndUpdateEntityMeta(newObject);

            newObject.Modified = newObject.Created;
            newObject.ModifiedAsMillisecondsEpochTime = newObject.CreatedAsMillisecondsEpochTime;
        }

        internal static void WalkGraphAndUpdateEntityMeta(object current)
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
                        if ((Guid)p.GetValue(current, null) == default(Guid))
                        {
                            p.SetValue(current, Guid.NewGuid(), null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.Created))
                    {
                        //set created datetime if this is null
                        if ((DateTime)p.GetValue(current, null) == default(DateTime))
                        {
                            p.SetValue(current, DateTime.UtcNow, null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.CreatedAsMillisecondsEpochTime))
                    {
                        //set created datetime if this is null
                        if (((double)p.GetValue(current, null)).Equals(default(double)))
                        {
                            p.SetValue(current, DateTime.UtcNow.ConvertToSecondsEpochTime(), null);
                        }
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
                        {
                            foreach (var sub in (IEnumerable)p.GetValue(current, null)) WalkGraphAndUpdateEntityMeta(sub);
                        }
                    }
            }
        }
    }
}