﻿namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    //methods return object after changes have been applied, including previous uncommitted session changes

    internal class DataStoreUpdateCapabilities
    {
        private readonly DataStoreOptions dataStoreOptions;

        private readonly IMessageAggregator eventAggregator;

        private readonly EventReplay eventReplay;

        private readonly IncrementVersions incrementVersions;

        public DataStoreUpdateCapabilities(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator eventAggregator,
            DataStoreOptions dataStoreOptions,
            IncrementVersions incrementVersions)
        {
            this.eventAggregator = eventAggregator;
            this.dataStoreOptions = dataStoreOptions;
            this.incrementVersions = incrementVersions;
            this.eventReplay = new EventReplay(eventAggregator);
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        // .. update using Id; get values from another instance of the same aggregate
        public Task<T> Update<T, O>(T src, O options, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
        {
            //clone, we don't want changes made at any point after this call, to affect the commit or the resulting events
            var cloned = src.Clone();

            //exclude these for the scenario where you try to update an object which
            //has been added in this session but has not yet been committed
            //because these values are set AFTER you pass the object to the datastore
            //if you passed to this function the original object you passed to the Create<T>() Function
            //it will attempt to overwrite the Created variables with NULL values from that instance
            var excludedParameters = new[]
            {
                nameof(IAggregate.Created),
                nameof(IAggregate.CreatedAsMillisecondsEpochTime),
                nameof(IAggregate.Modified),
                nameof(IAggregate.ModifiedAsMillisecondsEpochTime),
                nameof(IAggregate.VersionHistory)
            };

            return UpdateById<T, UpdateOptionsLibrarySide>(
                src.id,
                model => cloned.CopyPropertiesTo(model, excludedParameters),
                options,
                methodName);
        }

        public async Task<T> UpdateById<T, O>(Guid id, Action<T> action, O options, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
        {
            return (await UpdateWhere(x => x.id == id, action, options, methodName)).SingleOrDefault();
        }

        // update a DataObject selected with a singular predicate
        public async Task<IEnumerable<T>> UpdateWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            O options,
            string methodName = null) where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()

        {
            var objectsToUpdate = await this.eventAggregator
                                            .CollectAndForward(
                                                new AggregatesQueriedOperation<T>(
                                                    methodName,
                                                    DsConnection.CreateDocumentQuery<T>().Where(predicate))).To(DsConnection.ExecuteQuery)
                                            .ConfigureAwait(false);

            var dataObjects = this.eventReplay.ApplyAggregateEvents(objectsToUpdate, predicate.Compile()).AsEnumerable();

            return await UpdateInternal(action, dataObjects, methodName, options);
        }

        private async Task<IEnumerable<T>> UpdateInternal<T, O>(Action<T> action, IEnumerable<T> dataObjects, string methodName, O options)
            where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
        {
            foreach (var dataObject in dataObjects)
            {
                Guard.Against(dataObject.ReadOnly && !options.AllowReadonlyOverwriting, "Cannot update read-only item " + dataObject.id);

                DataStoreDeleteCapabilities.CheckWasObjectAlreadyHardDeleted<T>(this.eventAggregator, dataObject.id);
            }

            var clones = new List<T>();

            foreach (var dataObject in dataObjects)
            {
                var originalObject = dataObject.Clone();

                //* set here so changes are counted in the restrictedProperties calculation 
                DataStoreCreateCapabilities.WalkGraphAndUpdateEntityMeta(dataObject);

                var restrictedPropertiesBefore = originalObject.id + dataObject.Schema;
                var restrictedCreatedBefore =
                    dataObject.Created.ToString(CultureInfo.InvariantCulture) + dataObject.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedBefore = dataObject.Modified.ToString(CultureInfo.InvariantCulture)
                                               + dataObject.ModifiedAsMillisecondsEpochTime;
                var restrictedVersionInfo = dataObject.VersionHistory.ToJsonString();
                restrictedPropertiesBefore = restrictedPropertiesBefore + restrictedCreatedBefore + restrictedModifiedBefore
                                             + restrictedVersionInfo;

                action(dataObject);
                //* set here to override any resetting of restricted properties set only internally by the action()
                DataStoreCreateCapabilities.WalkGraphAndUpdateEntityMeta(dataObject);
                DisableOptimisticConcurrencyIfRequested(dataObject); //- has to happen after action

                var restrictedPropertiesAfter = originalObject.id + dataObject.Schema;
                var restrictedCreatedAfter =
                    dataObject.Created.ToString(CultureInfo.InvariantCulture) + dataObject.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedAfter = dataObject.Modified.ToString(CultureInfo.InvariantCulture)
                                              + dataObject.ModifiedAsMillisecondsEpochTime;
                var restrictedVersionInfoAfter = dataObject.VersionHistory.ToJsonString();
                restrictedPropertiesAfter = restrictedPropertiesAfter + restrictedCreatedAfter + restrictedModifiedAfter
                                            + restrictedVersionInfoAfter;

                Guard.Against(
                    restrictedPropertiesBefore != restrictedPropertiesAfter,
                    "Cannot change restricted properties [" + $"{nameof(Aggregate.id)}, {nameof(Aggregate.Schema)}, "
                                                            + $"{nameof(Aggregate.Created)}, {nameof(Aggregate.CreatedAsMillisecondsEpochTime)}, "
                                                            + $"{nameof(Aggregate.Modified)}, {nameof(Aggregate.ModifiedAsMillisecondsEpochTime)},  "
                                                            + $"{nameof(Aggregate.VersionHistory)} ] on Aggregate {originalObject.id}");

                dataObject.Modified = DateTime.UtcNow;
                dataObject.ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToSecondsEpochTime();

                var clone = dataObject.Clone();

                var etagUpdated = new Action<string>(newTag => clone.Etag = newTag);

                etagUpdated += newTag =>
                    {
                    var previousMatches = this.eventAggregator.AllMessages.OfType<QueuedUpdateOperation<T>>()
                                              .Where(q => q.Committed == false && q.AggregateId == dataObject.id).ToList();
                    previousMatches.ForEach(queuedUpdate => { queuedUpdate.NewModel.Etag = newTag; /* update other queued updates */ });
                    };

                this.eventAggregator.Collect(
                    new QueuedUpdateOperation<T>(
                        methodName,
                        dataObject,
                        originalObject,
                        DsConnection,
                        this.eventAggregator,
                        etagUpdated));

                await this.incrementVersions.IncrementAggregateVersionOfQueuedItem(dataObject, methodName);

                clone.Etag = "waiting to be committed";
                clones.Add(clone);
            }

            //- clone otherwise its to easy to change the referenced object before committing
            return clones;

            void DisableOptimisticConcurrencyIfRequested(T dataObject)
            {
                //- clearing eTag disables application in the repo's
                var dsOptionsConcurrencySetting = this.dataStoreOptions?.OptimisticConcurrency ?? true;
                if (options.OptimisticConcurrency == false || dsOptionsConcurrencySetting == false
                                                           || string.IsNullOrWhiteSpace(dataObject.Etag))
                {
                    dataObject.Etag = null;
                }
            }
        }
    }
}