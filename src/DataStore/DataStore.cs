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
        private readonly IMessageAggregator messageAggregator;

        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null)
        {
            this.messageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
            DsConnection = documentRepository;

            QueryCapabilities = new DataStoreQueryCapabilities(DsConnection, this.messageAggregator);
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, this.messageAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, this.messageAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, this.messageAggregator);
        }

        public IAdvancedCapabilities Advanced => new AdvancedCapabilities(DsConnection, this.messageAggregator);

        public IDocumentRepository DsConnection { get; }

        public CircuitBoard.IReadOnlyList<IDataStoreOperation> ExecutedOperations => new ReadOnlyCapableList<IDataStoreOperation>().Op(
            l => l.AddRange(this.messageAggregator.AllMessages.OfType<IDataStoreOperation>()));

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
            return new DataStoreWriteOnly<T>(DsConnection, this.messageAggregator);
        }

        public async Task CommitChanges()
        {
            var dataStoreEvents = this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(e => !e.Committed).ToList();

            foreach (var dataStoreWriteEvent in dataStoreEvents) await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
        }

        public Task<T> Create<T>(T model, bool readOnly = false) where T : class, IAggregate, new()
        {
            return CreateCapabilities.Create(model, readOnly);
        }

        public Task<T> DeleteHardById<T>(Guid id) where T : class, IAggregate, new()
        {
            return DeleteCapabilities.DeleteHardById<T>(id);
        }

        public Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            return DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public Task<T> DeleteSoftById<T>(Guid id) where T : class, IAggregate, new()
        {
            return DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            return DeleteCapabilities.DeleteSoftWhere(predicate);
        }

        public void Dispose()
        {
            DsConnection.Dispose();
        }

        public Task<bool> Exists(Guid id)
        {
            return QueryCapabilities.Exists(id);
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

        public Task<T> Update<T>(T src, bool overwriteReadOnly = true) where T : class, IAggregate, new()
        {
            return UpdateCapabilities.Update(src, overwriteReadOnly);
        }

        public Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : class, IAggregate, new()
        {
            return UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false)
            where T : class, IAggregate, new()
        {
            return UpdateCapabilities.UpdateWhere(predicate, action);
        }
    }
}