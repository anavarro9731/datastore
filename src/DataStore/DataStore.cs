namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;
    using Microsoft.Azure.Documents;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStore : IDataStore
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStore(IDocumentRepository documentRepository, IEventAggregator eventAggregator = null)
        {
            _eventAggregator = eventAggregator ?? new EventAggregator();
            DsConnection = documentRepository;
            QueryCapabilities = new DataStoreQueryCapabilities(DsConnection, _eventAggregator);
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, _eventAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, _eventAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, _eventAggregator);
        }


        public IDocumentRepository DsConnection { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreQueryCapabilities QueryCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        #region IDataStore Members

        public Task<T> Create<T>(T model, bool readOnly = false) where T : IAggregate, new()
        {
            return CreateCapabilities.Create(model, readOnly);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            return await DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T : IAggregate
        {
            return await DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            return await DeleteCapabilities.DeleteHardById<T>(id);
        }


        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
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

        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            return await QueryCapabilities.Read(queryableExtension);
        }

        public async Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension) where T : IAggregate
        {
            return await QueryCapabilities.ReadActive(queryableExtension);
        }

        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            return await QueryCapabilities.ReadActiveById<T>(modelId);
        }

        public async Task<Document> ReadById(Guid modelId)
        {
            return await QueryCapabilities.ReadById(modelId);
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : IAggregate
        {
            return await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public async Task<T> Update<T>(T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            return await UpdateCapabilities.Update(src, overwriteReadOnly);
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : IAggregate
        {
            return await UpdateCapabilities.UpdateWhere(predicate, action);
        }

        public IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : IAggregate, new()
        {
            return new DataStoreWriteOnly<T>(DsConnection, _eventAggregator);
        }

        public IDataStoreQueryCapabilities AsReadOnly()
        {
            return QueryCapabilities;
        }

        #endregion

        public void CommitChanges()
        {
            // TODO: apply all events
            // this requires an update to merge events in read queries made before committing
        }
    }
}