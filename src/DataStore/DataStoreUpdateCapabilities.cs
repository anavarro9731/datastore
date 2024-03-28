namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Options.LibrarySide;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    #endregion

    //methods return object after changes have been applied, including previous uncommitted session changes

    internal class DataStoreUpdateCapabilities
    {
        private readonly IDataStoreOptions dataStoreOptions;

        private readonly IMessageAggregator eventAggregator;

        private readonly EventReplay eventReplay;

        private readonly IncrementVersions incrementVersions;

        public DataStoreUpdateCapabilities(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator eventAggregator,
            IDataStoreOptions dataStoreOptions,
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
        public Task<T> Update<T, O>(T src, O options, string methodName = null) where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
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
                nameof(IAggregate.VersionHistory),
                nameof(IAggregate.PartitionKey),
                nameof(IAggregate.PartitionKeys)
            };

            return UpdateById<T, UpdateOptionsLibrarySide>(src.id, model => cloned.CopyPropertiesTo(model, excludedParameters), options, methodName);
        }

        public async Task<T> UpdateById<T, O>(Guid id, Action<T> action, O options, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
        {
            var matchingObjectFromDb = await this.eventAggregator.CollectAndForward(new AggregateQueriedByIdOperationOperation<T>(methodName, id, options))
                                                 .To(DsConnection.GetItemAsync<T>).ConfigureAwait(false);

            
            
            return (await ProcessUpdateOfObjects(
                        x => x.id == id,
                        action,
                        options,
                        methodName,
                        matchingObjectFromDb == default
                            ? Array.Empty<T>()
                            : new[]
                            {
                                matchingObjectFromDb
                            }).ConfigureAwait(false)).SingleOrDefault();
        }

        //* update a DataObject selected with a singular predicate
        public async Task<IEnumerable<T>> UpdateWhere<T, O>(Expression<Func<T, bool>> predicate, Action<T> action, O options, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()

        {
            {
                var queryable = DsConnection.CreateQueryable<T>(options);
                if (predicate != default) queryable = queryable.Where(predicate);

                var matchingObjectsFromDb = await this.eventAggregator.CollectAndForward(new AggregatesQueriedOperation<T>(methodName, queryable, options))
                                                      .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

                return await ProcessUpdateOfObjects(predicate, action, options, methodName, matchingObjectsFromDb).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<T>> ProcessUpdateOfObjects<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            O options,
            string methodName,
            IEnumerable<T> matchingObjectsFromDb) where T : class, IAggregate, new() where O : UpdateOptionsLibrarySide, new()
        {
            void DisableOptimisticConcurrencyIfRequested(T dataObject)
            {
                //- clearing eTag disables application in the repo's
                var dsOptionsConcurrencySetting = this.dataStoreOptions?.OptimisticConcurrency ?? true;
                if (options.OptimisticConcurrency == false || dsOptionsConcurrencySetting == false || string.IsNullOrWhiteSpace(dataObject.Etag))
                {
                    dataObject.Etag = null;
                }
            }

            void PerformUpdateOfProperties(ref T objectToUpdate)
            {
                //* set here so changes are counted in the restrictedProperties calculation 
                objectToUpdate.WalkGraphAndUpdateEntityMeta();

                var originalObjectId = objectToUpdate.id;

                var restrictedIdBefore = objectToUpdate.id + objectToUpdate.Schema + objectToUpdate.PartitionKey + objectToUpdate.PartitionKeys.ToJsonString();
                var restrictedCreatedBefore = objectToUpdate.Created.ToString(CultureInfo.InvariantCulture) + objectToUpdate.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedBefore = objectToUpdate.Modified.ToString(CultureInfo.InvariantCulture) + objectToUpdate.ModifiedAsMillisecondsEpochTime;
                var restrictedVersionInfo = objectToUpdate.VersionHistory.ToJsonString();
                restrictedIdBefore = restrictedIdBefore + restrictedCreatedBefore + restrictedModifiedBefore + restrictedVersionInfo;

                action(objectToUpdate);
                //* set here to override any resetting of restricted properties set only internally by the action()
                objectToUpdate.WalkGraphAndUpdateEntityMeta();
                DisableOptimisticConcurrencyIfRequested(objectToUpdate); //- has to happen after action

                var restrictedIdAfter = objectToUpdate.id + objectToUpdate.Schema + objectToUpdate.PartitionKey + objectToUpdate.PartitionKeys.ToJsonString();
                var restrictedCreatedAfter = objectToUpdate.Created.ToString(CultureInfo.InvariantCulture) + objectToUpdate.CreatedAsMillisecondsEpochTime;
                var restrictedModifiedAfter = objectToUpdate.Modified.ToString(CultureInfo.InvariantCulture) + objectToUpdate.ModifiedAsMillisecondsEpochTime;
                var restrictedVersionInfoAfter = objectToUpdate.VersionHistory.ToJsonString();
                restrictedIdAfter = restrictedIdAfter + restrictedCreatedAfter + restrictedModifiedAfter + restrictedVersionInfoAfter;

                Guard.Against(
                    restrictedIdBefore != restrictedIdAfter,
                    "Cannot change restricted properties ["
                    + $"{nameof(Aggregate.id)}, {nameof(Aggregate.Schema)}, {nameof(objectToUpdate.PartitionKey)}, {nameof(objectToUpdate.PartitionKeys)} "
                    + $"{nameof(Aggregate.Created)}, {nameof(Aggregate.CreatedAsMillisecondsEpochTime)}, "
                    + $"{nameof(Aggregate.Modified)}, {nameof(Aggregate.ModifiedAsMillisecondsEpochTime)},  "
                    + $"{nameof(Aggregate.VersionHistory)} ] on Aggregate {originalObjectId}");

                objectToUpdate.Modified = DateTime.UtcNow;
                objectToUpdate.ModifiedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime();
            }

            var matchingObjectsDbAndQueued = this.eventReplay.ApplyQueuedOperations(matchingObjectsFromDb, predicate.Compile());
            
            var results = new List<T>(); //* return a list of clones

            foreach (var originalObject in matchingObjectsDbAndQueued)
            {
                Guard.Against(originalObject.ReadOnly && !options.AllowReadonlyOverwriting, "Cannot update read-only item " + originalObject.id);

                //* check to see if this operation can this be merged into a previous operation
                var modelQueuedForPersistence = this.eventReplay.MergeCurrentUpdateIntoPreviousCreateOrUpdateOperations<T>(
                    originalObject.id,
                    model => PerformUpdateOfProperties(ref model),
                    methodName);

                if (modelQueuedForPersistence != null)
                {
                    var itemToReturnToCaller =
                        modelQueuedForPersistence.Clone(); //* return clones otherwise its to easy to change the referenced object before committing 
                    itemToReturnToCaller.Etag = "waiting to be committed";
                    (modelQueuedForPersistence as IEtagUpdated).EtagUpdated += s => { itemToReturnToCaller.Etag = s; };
                    results.Add(itemToReturnToCaller);
                }
                else
                {
                    var modelToPersist =
                        originalObject
                            .Clone(); //* clones originalObject so it will always be correct when used as the BeforeModel in the QueuedUpdateOperation below.
                    PerformUpdateOfProperties(ref modelToPersist);

                    await this.incrementVersions.IncrementAggregateVersionOfItemToBeQueued(modelToPersist, methodName).ConfigureAwait(false);

                    this.eventAggregator.Collect(new QueuedUpdateOperation<T>(methodName, modelToPersist, originalObject, options, DsConnection, this.eventAggregator));

                    var itemToReturnToCaller = modelToPersist.Clone(); //* return clones otherwise its to easy to change the referenced object before committing 
                    itemToReturnToCaller.Etag = "waiting to be committed";
                    results.Add(itemToReturnToCaller);
                    (modelToPersist as IEtagUpdated).EtagUpdated += newTag => itemToReturnToCaller.Etag = newTag;
                }
            }

            return results;
        }
    }
}