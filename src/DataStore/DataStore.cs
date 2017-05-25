﻿namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Interfaces.LowLevel;
    using MessageAggregator;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStore : IDataStore
    {
        private readonly IMessageAggregator messageAggregator;

        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null)
        {
            messageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
            DsConnection = documentRepository;

            QueryCapabilities = new DataStoreQueryCapabilities(DsConnection, messageAggregator);
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, messageAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, messageAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, messageAggregator);
        }

        public IDocumentRepository DsConnection { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreQueryCapabilities QueryCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        public ServiceApi.Interfaces.LowLevel.MessageAggregator.IReadOnlyList<IDataStoreOperation> ExecutedOperations =>
            new ReadOnlyCapableList<IDataStoreOperation>().Op(
                l => l.AddRange(messageAggregator.AllMessages.OfType<IDataStoreOperation>()));

        public ServiceApi.Interfaces.LowLevel.MessageAggregator.IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations =>
            new ReadOnlyCapableList<IQueuedDataStoreWriteOperation>().Op(l => l.AddRange(messageAggregator.AllMessages
                .OfType<IQueuedDataStoreWriteOperation>()
                .Where(o => o.Committed == false)));

        public IAdvancedCapabilities Advanced => new AdvancedCapabilities(DsConnection, messageAggregator);

        #region

        public async Task CommitChanges()
        {
            var dataStoreEvents = messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>()
                .Where(e => !e.Committed)
                .ToList();

            foreach (var dataStoreWriteEvent in dataStoreEvents)
                await dataStoreWriteEvent.CommitClosure();
        }

        public Task<T> Create<T>(T model, bool readOnly = false) where T : class, IAggregate, new()
        {
            return CreateCapabilities.Create(model, readOnly);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate)
            where T : class, IAggregate, new()
        {
            return await DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : class, IAggregate, new()
        {
            return await DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public async Task<T> DeleteHardById<T>(Guid id) where T : class, IAggregate, new()
        {
            return await DeleteCapabilities.DeleteHardById<T>(id);
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate)
            where T : class, IAggregate, new()
        {
            return await DeleteCapabilities.DeleteSoftWhere(predicate);
        }

        public void Dispose()
        {
            DsConnection.Dispose();
        }

        public async Task<bool> Exists(Guid id)
        {
            return await QueryCapabilities.Exists(id);
        }

        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : class, IAggregate, new()
        {
            return await QueryCapabilities.Read(queryableExtension);
        }

        public async Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : class, IAggregate, new()
        {
            return await QueryCapabilities.ReadActive(queryableExtension);
        }

        public async Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            return await QueryCapabilities.ReadActiveById<T>(modelId);
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true)
            where T : class, IAggregate, new()
        {
            return await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public async Task<T> Update<T>(T src, bool overwriteReadOnly = true)
            where T : class, IAggregate, new()
        {
            return await UpdateCapabilities.Update(src, overwriteReadOnly);
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : class, IAggregate, new()
        {
            return await UpdateCapabilities.UpdateWhere(predicate, action);
        }

        public IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new()
        {
            return new DataStoreWriteOnly<T>(DsConnection, messageAggregator);
        }

        public IDataStoreQueryCapabilities AsReadOnly()
        {
            return QueryCapabilities;
        }

        #endregion
    }
}