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
        private readonly DataStoreOptions dataStoreOptions;

        private readonly IMessageAggregator messageAggregator;

        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null, DataStoreOptions dataStoreOptions = null)
        {
            {
                ValidateOptions(dataStoreOptions);

                {
                    // init vars
                    this.messageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
                    this.dataStoreOptions = dataStoreOptions ?? new DataStoreOptions();
                    DsConnection = documentRepository;

                    QueryCapabilities = new DataStoreQueryCapabilities(DsConnection, this.messageAggregator);
                    UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, this.messageAggregator);
                    DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, this.messageAggregator);
                    CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, this.messageAggregator);
                }
            }

            void ValidateOptions(DataStoreOptions options)
            {
                //not sure how to handle disabling version history when its already been enabled??
            }
        }

        public IAdvancedCapabilities Advanced => new AdvancedCapabilities(DsConnection, this.messageAggregator);

        public IDocumentRepository DsConnection { get; }

        public CircuitBoard.IReadOnlyList<IDataStoreOperation> ExecutedOperations =>
            new ReadOnlyCapableList<IDataStoreOperation>().Op(l => l.AddRange(this.messageAggregator.AllMessages.OfType<IDataStoreOperation>()));

        public CircuitBoard.IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations =>
            new ReadOnlyCapableList<IQueuedDataStoreWriteOperation>().Op(
                l => l.AddRange(this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(o => o.Committed == false)));

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
                var committableEvents = this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(e => !e.Committed).ToList();

                foreach (var dataStoreWriteEvent in committableEvents)
                    await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
            }
        }

        public async Task<T> Create<T>(T model, bool readOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(Create);

            var result = await CreateCapabilities.Create(model, readOnly, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.'+ nameof(DeleteHardById);

            var result = await DeleteCapabilities.DeleteHardById<T>(id, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(DeleteHardWhere);

            var results = await DeleteCapabilities.DeleteHardWhere(predicate, methodName).ConfigureAwait(false);

            foreach (var result in results)
                await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(DeleteSoftById);

            var result = await DeleteCapabilities.DeleteSoftById<T>(id, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(DeleteSoftWhere);

            var results = await DeleteCapabilities.DeleteSoftWhere(predicate, methodName).ConfigureAwait(false);

            foreach (var result in results)
                await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        public void Dispose()
        {
            DsConnection.Dispose();
        }

        public Task<bool> Exists(Guid id)
        {
            return QueryCapabilities.Exists(id);
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
                    UnitOfWorkResponsibleForStateChange = this.dataStoreOptions.UnitOfWorkId
                }, methodName: methodName);

            //get the history index record
            var historyIndexRecord = (await QueryCapabilities.ReadActive<AggregateHistory<T>>(h => h.AggregateId == model.id)).SingleOrDefault();

            //prepare the new header record
            var historyItemHeader = new AggregateHistoryItemHeader
            {
                AssemblyQualifiedTypeName = model.GetType().AssemblyQualifiedName,
                UnitWorkId = this.dataStoreOptions.UnitOfWorkId.GetValueOrDefault(),
                VersionedAt = DateTime.UtcNow,
                VersionId = historyIndexRecord?.Version + 1 ?? 1,
                AggegateHistoryItemId = historyItemId
            };

            if (historyIndexRecord == null)
            {
                //create index record
                await CreateCapabilities.Create(
                    new AggregateHistory<T>
                    {
                        Version = 1,
                        AggregateVersions = new List<IAggregateHistoryItemHeader>
                        {
                            historyItemHeader
                        },
                        AggregateId = model.id
                    }, methodName: methodName);
            }
            else
            {
                //add header to existing record
                historyIndexRecord.AggregateVersions.Add(historyItemHeader);
                historyIndexRecord.Version = historyIndexRecord.AggregateVersions.Count;
                //and update
                await UpdateCapabilities.Update(historyIndexRecord, methodName: methodName);
            }
        }

        public Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            return QueryCapabilities.Read(predicate);
        }

        public Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            return QueryCapabilities.ReadActive(predicate);
        }

        public Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            return QueryCapabilities.ReadActiveById<T>(modelId);
        }

        public async Task<T> Update<T>(T src,  bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(Update);

            var result = await UpdateCapabilities.Update(src, overwriteReadOnly, methodName).ConfigureAwait(false);
            
            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);
            
            return result;
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action,  bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(UpdateById);

            var result = await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new()
        {
            methodName += '.' + nameof(UpdateWhere);

            var results = await UpdateCapabilities.UpdateWhere(predicate, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            foreach (var result in results)
                await IncrementAggregateHistoryIfEnabled(result, $"{methodName}.{nameof(IncrementAggregateHistory)}").ConfigureAwait(false);

            return results;
        }

        private Task IncrementAggregateHistoryIfEnabled<T>(T model, string methodName) where T : class, IAggregate, new()
        {
            if (this.dataStoreOptions.UseVersionHistory)
            {
                return IncrementAggregateHistory(model, methodName);
            }

            return Task.CompletedTask;
        }
    }

    public class DataStoreOptions
    {
        public Guid? UnitOfWorkId { get; set; }

        public bool UseVersionHistory { get; set; }
    }
}