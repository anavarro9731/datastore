namespace DataStore
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    public class IncrementVersions
    {
        private readonly DataStore dataStore;

        public IncrementVersions(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task IncrementAggregateVersionOfQueuedItem<T>(T model, string methodName) where T : class, IAggregate, new()
        {
            {
                if (!typeof(T).InheritsOrImplements(typeof(AggregateHistoryItem<>))) //- without this check you'd get recursion to infinity
                {
                    PrepareNewHistoryRecordEntry(out var historyRecordEntry);

                    AddHistoryRecordEntryToExistingIndexOfQueuedItem(model, historyRecordEntry);

                    await CreateFullAggregateRecordIfEnabled(model, historyRecordEntry, $"{methodName}.{nameof(IncrementAggregateVersionOfQueuedItem)}");
                    
                }
            }

            void PrepareNewHistoryRecordEntry(out Aggregate.AggregateVersionInfo historyRecordEntry)
            {
                historyRecordEntry = new Aggregate.AggregateVersionInfo
                {
                    AssemblyQualifiedTypeName = model.GetType().AssemblyQualifiedName,
                    UnitOfWorkId = this.dataStore.DataStoreOptions.UnitOfWorkId,
                    AggegateHistoryItemId =
                        VersioningStyleIsFull()
                            ? Guid.NewGuid()
                            : (Guid?)null,
                    CommitBatch = this.dataStore.ExecutedOperations.OfType<IDataStoreWriteOperation>().Any()
                                      ? this.dataStore.ExecutedOperations.OfType<IDataStoreWriteOperation>()
                                            .Max(x => x.GetHistoryItems.Any() ? x.GetHistoryItems.Max(y => y.CommitBatch) : 0) + 1
                                      : 1 /* this is rather confusing, the reason history items can sometimes be empty is that
                                      if you are dealing with a historyitem itself there won't be any*/
                };
            }

            void AddHistoryRecordEntryToExistingIndexOfQueuedItem(T aggregate, Aggregate.AggregateVersionInfo newVersion)
            {
                aggregate.VersionHistory.Add(newVersion);
            }
            bool VersioningStyleIsFull()
            {
                return this.dataStore.DataStoreOptions.VersionHistory.VersioningStyle == DataStoreOptions.VersioningStyle.CompleteCopyOfAllAggregateVersions;
            }

            async Task CreateFullAggregateRecordIfEnabled(T modelNested, Aggregate.AggregateVersionInfo historyRecordEntry, string methodNameNested)
            {
                {
                    if (VersioningStyleIsFull())
                    {
                        await CreateFullAggregateRecord(historyRecordEntry.AggegateHistoryItemId.Value).ConfigureAwait(false);
                    }
                }

      
                async Task CreateFullAggregateRecord(Guid id)
                {
                    await this.dataStore.Create(
                        new AggregateHistoryItem<T>
                        {
                            id = id,
                            AggregateVersion = modelNested
                            /* this ref is shared with the actual write any changes will be synchronised
                             since this is precommit and that may be fine but worth remembering */
                        },
                        methodName: methodNameNested).ConfigureAwait(false);
                }
            }
        }

        public async Task DeleteAggregateHistory<T>(Guid id, string methodName) where T : class, IAggregate, new()
        {
            if (typeof(T) != typeof(AggregateHistoryItem<>)) //- without this check you'd get recursion to infinity
            //- delete history records
                await this.dataStore.DeleteHardWhere<AggregateHistoryItem<T>>(h => h.AggregateVersion.id == id, $"{methodName}.{nameof(DeleteAggregateHistory)}")
                      .ConfigureAwait(false);
            ;
        }
    }
}