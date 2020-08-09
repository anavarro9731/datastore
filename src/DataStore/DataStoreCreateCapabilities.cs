using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DataStore.Tests")]

namespace DataStore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    //methods return the enriched object as it was added to the database

    internal class DataStoreCreateCapabilities
    {
        private readonly IncrementVersions incrementVersions;

        private readonly IMessageAggregator messageAggregator;

        internal static void ForceProperties<T>(bool readOnly, T newObject) where T : class, IAggregate, new()
        {
            newObject.Schema =
                typeof(T).FullName; //will be defaulted by Aggregate but needs to be forced as it is open to change because of serialisation opening the property setter
            newObject.ReadOnly = readOnly;
            newObject.VersionHistory =
                new List<Aggregate.AggregateVersionInfo>(); //-set again in commitchanges, still best not to allow any invalid state

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
                        if ((Guid)p.GetValue(current, null) == default)
                        {
                            p.SetValue(current, Guid.NewGuid(), null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.Created))
                    {
                        //set created datetime if this is null
                        if ((DateTime)p.GetValue(current, null) == default)
                        {
                            p.SetValue(current, DateTime.UtcNow, null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.CreatedAsMillisecondsEpochTime))
                    {
                        //set created datetime if this is null
                        if (((double)p.GetValue(current, null)).Equals(default))
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

        public DataStoreCreateCapabilities(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator messageAggregator,
            IncrementVersions incrementVersions)
        {
            this.messageAggregator = messageAggregator;
            this.incrementVersions = incrementVersions;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> Create<T, O>(T model, O options, string methodName = null)
            where T : class, IAggregate, new() where O : CreateOptionsLibrarySide, new()
        {
            //create a new one, we definitely don't want to use the instance passed in, in the event it changes after this call
            //and affects the commit and/or the resulting events
            var newObject = model.Clone();

            ForceProperties(options.SetReadonlyFlag, newObject);

            Guard.Against(
                this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>()
                    .SingleOrDefault(e => !e.Committed && e.AggregateId == newObject.id) != null,
                $"An item with the same ID of {newObject.id} is already queued to be created",
                Guid.Parse("63328bcd-d58d-446a-bc85-fedfde43d2e2"));

            var count = await DsConnection.CountAsync(new AggregateCountedOperation<T>(methodName, t => t.id == newObject.id))
                                          .ConfigureAwait(false);

            var existsAlready = count > 0;
            Guard.Against(
                existsAlready,
                $"An item with the same ID of {newObject.id} already exists",
                Guid.Parse("cfe3ebc2-4677-432b-9ded-0ef498b9f59d"));

            this.messageAggregator.Collect(
                new QueuedCreateOperation<T>(methodName, newObject, DsConnection, this.messageAggregator));
            
            var itemToReturnToCaller = newObject.Clone(); //* return clones otherwise its to easy to change the referenced object before committing    
            itemToReturnToCaller.Etag = "waiting to be committed";
            (newObject as IEtagUpdated).EtagUpdated += newTag => itemToReturnToCaller.Etag = newTag; 
            
            await this.incrementVersions.IncrementAggregateVersionOfItemToBeQueued(newObject, methodName);

            //for the same reason as the above we want a new object, but we want to return the enriched one, so we clone it,
            //essentially no external client should be able to get a reference to the instance we use internally
            return itemToReturnToCaller;
        }
    }
}