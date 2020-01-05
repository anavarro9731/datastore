namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.PureFunctions.Extensions;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStore : IDataStore
    {
        public DataStoreOptions DataStoreOptions { get; }

        public IMessageAggregator MessageAggregator { get; }

        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null, DataStoreOptions dataStoreOptions = null)
        {
            {
                ValidateOptions(dataStoreOptions);

                {
                    // init vars
                    this.MessageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
                    this.DataStoreOptions = dataStoreOptions ?? DataStoreOptions.Create();
                    DocumentRepository = documentRepository;

                    QueryCapabilities = new DataStoreQueryCapabilities(DocumentRepository, this.MessageAggregator);
                    UpdateCapabilities = new DataStoreUpdateCapabilities(DocumentRepository, this.MessageAggregator);
                    DeleteCapabilities = new DataStoreDeleteCapabilities(DocumentRepository, UpdateCapabilities, this.MessageAggregator);
                    CreateCapabilities = new DataStoreCreateCapabilities(DocumentRepository, this.MessageAggregator);
                }
            }

            void ValidateOptions(DataStoreOptions options)
            {
                //not sure how to handle disabling version history when its already been enabled??
            }
        }

        public IDocumentRepository DocumentRepository { get; }

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations =>
            new ReadOnlyCapableList<IDataStoreOperation>().Op(l => l.AddRange(this.MessageAggregator.AllMessages.OfType<IDataStoreOperation>()));

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations =>
            new ReadOnlyCapableList<IQueuedDataStoreWriteOperation>().Op(
                l => l.AddRange(this.MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(o => o.Committed == false)));

        public IWithoutEventReplay WithoutEventReplay => new WithoutEventReplay(DocumentRepository, this.MessageAggregator);

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreQueryCapabilities QueryCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        public IDataStoreQueryCapabilities AsReadOnly()
        {
            return QueryCapabilities;
        }

        public IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new()
        {
            return new DataStoreWriteOnly<T>(this);
        }

        public async Task CommitChanges()
        {
            await CommittableEvents().ConfigureAwait(false);

            async Task CommittableEvents()
            {
                var committableEvents = this.MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(e => !e.Committed).ToList();

                foreach (var dataStoreWriteEvent in committableEvents)
                    await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
            }
        }

        public async Task<T> Create<T>(T model, bool readOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Create);

            var result = await CreateCapabilities.Create(model, readOnly, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHardById);

            var result = await DeleteCapabilities.DeleteHardById<T>(id, methodName).ConfigureAwait(false);

            if (result != null)
            {
                await DeleteAggregateHistory<T>(result.id, $"{methodName}.{nameof(DeleteAggregateHistory)}").ConfigureAwait(false);
            }

            return result;
        }

        public async Task<T> DeleteHard<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHard);

            var result = await DeleteCapabilities.DeleteHard(instance, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteSoft<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoft);

            var result = await DeleteCapabilities.DeleteSoft(instance, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHardWhere);

            var results = await DeleteCapabilities.DeleteHardWhere(predicate, methodName).ConfigureAwait(false);

            foreach (var result in results)
                await DeleteAggregateHistory<T>(result.id, $"{methodName}.{nameof(DeleteAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoftById);

            var result = await DeleteCapabilities.DeleteSoftById<T>(id, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoftWhere);

            var results = await DeleteCapabilities.DeleteSoftWhere(predicate, methodName).ConfigureAwait(false);
                
            foreach (var result in results)
                await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        public void Dispose()
        {
            DocumentRepository.Dispose();
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Read);

            var result = await QueryCapabilities.Read(predicate, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> Read<T>(string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Read);

            var result = await QueryCapabilities.Read<T>(methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> ReadById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(ReadById);

            var result = await QueryCapabilities.ReadById<T>(modelId, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(ReadActive);

            var result = await QueryCapabilities.ReadActive(predicate, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> ReadActive<T>(string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(ReadActive);

            var result = await QueryCapabilities.ReadActive<T>(methodName).ConfigureAwait(false);
                
            return result;
        }

        public async Task<T> ReadActiveById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(ReadActiveById);

            var result = await QueryCapabilities.ReadActiveById<T>(modelId, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> Update<T>(T src, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Update);

            var result = await UpdateCapabilities.Update(src, overwriteReadOnly, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(UpdateById);

            var result = await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false,
            string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(UpdateWhere);

            var results = await UpdateCapabilities.UpdateWhere(predicate, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            foreach (var result in results)
                await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        private async Task DeleteAggregateHistory<T>(Guid id, string methodName) where T : class, IAggregate, new()
        {
            //delete index record
            await DeleteCapabilities.DeleteHardWhere<AggregateHistory>(h => h.AggregateId == id, methodName).ConfigureAwait(false);
            //delete history records
            await DeleteCapabilities.DeleteHardWhere<AggregateHistoryItem<T>>(h => h.AggregateVersion.id == id, methodName).ConfigureAwait(false);
        }

        private async Task IncrementAggregateHistory<T>(T model, string methodName) where T : class, IAggregate, new()
        {
            //create the new history record
            Guid historyItemId;

            await CreateCapabilities.Create(
                new AggregateHistoryItem<T>
                {
                    id = historyItemId = Guid.NewGuid(),
                    AggregateVersion = model, //perhaps this needs to be cloned but i am not sure yet the consequence of not doing which would yield better perf
                    UnitOfWorkResponsibleForStateChange = this.DataStoreOptions.VersionHistory.UnitOfWorkId
                },
                methodName: methodName).ConfigureAwait(false);

            //get the history index record
            var historyIndexRecord =
                (await QueryCapabilities.ReadActive<AggregateHistory>(h => h.AggregateId == model.id).ConfigureAwait(false)).SingleOrDefault();

            //prepare the new header record
            var historyItemHeader = new AggregateHistoryItemHeader
            {
                AssemblyQualifiedTypeName = model.GetType().AssemblyQualifiedName,
                UnitWorkId = this.DataStoreOptions.VersionHistory.UnitOfWorkId.GetValueOrDefault(),
                VersionedAt = DateTime.UtcNow,
                VersionId = historyIndexRecord?.Version + 1 ?? 1,
                AggegateHistoryItemId = historyItemId
            };

            if (historyIndexRecord == null)
            {
                //create index record
                await CreateCapabilities.Create(
                    new AggregateHistory
                    {
                        Version = 1,
                        AggregateVersions = new List<AggregateHistoryItemHeader>
                        {
                            historyItemHeader
                        },
                        AggregateId = model.id
                    },
                    methodName: methodName).ConfigureAwait(false);
            }
            else
            {
                //add header to existing record
                historyIndexRecord.AggregateVersions.Add(historyItemHeader);
                historyIndexRecord.Version = historyIndexRecord.AggregateVersions.Count;
                //and update
                await UpdateCapabilities.Update(historyIndexRecord, methodName: methodName).ConfigureAwait(false);
            }
        }

        private Task IncrementAggregateHistoryIfEnabled<T>(T model, string methodName) where T : class, IAggregate, new()
        {
            if (this.DataStoreOptions.VersionHistory != null)
            {
                return IncrementAggregateHistory(model, methodName);
            }

            return Task.CompletedTask;
        }
    }
}