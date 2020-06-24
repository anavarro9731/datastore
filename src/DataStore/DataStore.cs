namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStore : IDataStore
    {
        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null, DataStoreOptions dataStoreOptions = null)
        {
            {
                ValidateOptions(dataStoreOptions);

                {
                    // init vars
                    MessageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
                    DataStoreOptions = dataStoreOptions ?? DataStoreOptions.Create();
                    DocumentRepository = documentRepository;

                    var incrementVersions = new IncrementVersions(this);

                    QueryCapabilities = new DataStoreQueryCapabilities(DocumentRepository, MessageAggregator);
                    UpdateCapabilities = new DataStoreUpdateCapabilities(DocumentRepository, MessageAggregator, DataStoreOptions, incrementVersions);
                    DeleteCapabilities = new DataStoreDeleteCapabilities(DocumentRepository, UpdateCapabilities, MessageAggregator, incrementVersions);
                    CreateCapabilities = new DataStoreCreateCapabilities(DocumentRepository, MessageAggregator, incrementVersions);
                }
            }

            void ValidateOptions(DataStoreOptions options)
            {
                //not sure how to handle disabling version history when its already been enabled??
            }
        }

        public DataStoreOptions DataStoreOptions { get; }

        public IDocumentRepository DocumentRepository { get; }

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations => MessageAggregator.AllMessages.OfType<IDataStoreOperation>().ToList().AsReadOnly();

        public IMessageAggregator MessageAggregator { get; }

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations
        {
            get
            {
                var queued = 
                    MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>()
                                     .Where(o => o.Committed == false).ToList().AsReadOnly();

                return queued;
            }
        }

        public IWithoutEventReplay WithoutEventReplay => new WithoutEventReplay(DocumentRepository, MessageAggregator);

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
            {
                FilterEvents(out var committableEvents, out var committedEvents);

                await CommitAllEvents(committableEvents);

            }

            async Task CommitAllEvents(List<IQueuedDataStoreWriteOperation> committableEvents)
            {
            
                foreach (var dataStoreWriteEvent in committableEvents)
                    await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
            }

            void FilterEvents(out List<IQueuedDataStoreWriteOperation> committableEvents, out List<IQueuedDataStoreWriteOperation> committedEvents)
            {
                var dsEvents = MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().ToList();
                committableEvents = dsEvents.Where(e => !e.Committed).ToList();
                committedEvents = dsEvents.Where(e => e.Committed).ToList();
            }
        }

        public async Task<T> Create<T>(T model, bool readOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Create);

            var result = await CreateCapabilities.Create(model, readOnly, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteHard<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHard);

            var result = await DeleteCapabilities.DeleteHard(instance, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHardById);

            var result = await DeleteCapabilities.DeleteHardById<T>(id, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteHardWhere);

            var results = await DeleteCapabilities.DeleteHardWhere(predicate, methodName).ConfigureAwait(false);

            return results;
        }

        public async Task<T> DeleteSoft<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoft);

            var result = await DeleteCapabilities.DeleteSoft(instance, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoftById);

            var result = await DeleteCapabilities.DeleteSoftById<T>(id, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(DeleteSoftWhere);

            var results = await DeleteCapabilities.DeleteSoftWhere(predicate, methodName).ConfigureAwait(false);

            return results;
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

        public async Task<T> ReadById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(ReadById);

            var result = await QueryCapabilities.ReadById<T>(modelId, methodName).ConfigureAwait(false);

            return result;
        }

        public async Task<T> Update<T, O>(T src, Action<O> setOptions, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(Update);

            var result = await UpdateCapabilities.Update(src, overwriteReadOnly, methodName).ConfigureAwait(false);

            return result;
        }

        public Task<T> Update<T>(T src, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            return Update<T, UpdateOptions>(src, options => { }, overwriteReadOnly, methodName);
        }

        public async Task<T> UpdateById<T, O>(Guid id, Action<T> action, Action<O> setOptions, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(UpdateById);

            var result = await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            return result;
        }

        public Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true, string methodName = null) where T : class, IAggregate, new()
        {
            return UpdateById<T, UpdateOptions>(id, action, options => { }, overwriteReadOnly, methodName);
        }

        public async Task<IEnumerable<T>> UpdateWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<O> setOptions,
            bool overwriteReadOnly = false,
            string methodName = null) where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            methodName = (methodName == null ? string.Empty : ".") + nameof(UpdateWhere);

            var results = await UpdateCapabilities.UpdateWhere(predicate, action, overwriteReadOnly, methodName).ConfigureAwait(false);

            return results;
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new()
        {
            return UpdateWhere<T, UpdateOptions>(predicate, action, options => { }, overwriteReadOnly, methodName);
        }


    }
}