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
        private readonly IEventAggregator eventAggregator;

        public DataStore(IDocumentRepository documentRepository, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DsConnection = documentRepository;
            QueryCapabilities = new DataStoreQueryCapabilities(DsConnection);
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, eventAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, eventAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, eventAggregator);
        }

        public IDocumentRepository DsConnection { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreQueryCapabilities QueryCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        public void CommitChanges()
        {
            // TODO: apply all events
            // this requires a re-work to merge events in read queries made before committing
        }

        public Task<T> Create<T>(T model, bool readOnly = false) where T : IAggregate, new()
        {
            return this.CreateCapabilities.Create(model, readOnly);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            return await this.DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public async Task<T> DeleteSoftById<T>(Guid id) where T: IAggregate
        {
            return await this.DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public async Task<T> DeleteHardById<T>(Guid id) where T : IAggregate
        {
            return await this.DeleteCapabilities.DeleteHardById<T>(id);
        }


        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            return await this.DeleteCapabilities.DeleteSoftWhere(predicate);
        }

        public void Dispose()
        {
            this.DsConnection.Dispose();
        }

        public async Task<bool> Exists(Guid id)
        {
            return await this.QueryCapabilities.Exists(id);
        }

        public async Task<IEnumerable<T>> Read<T>(Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            return await this.QueryCapabilities.Read(queryableExtension);
        }

        public async Task<IEnumerable<T>> ReadActive<T>(
            Func<IQueryable<T>, IQueryable<T>> queryableExtension) where T : IAggregate
        {
            return await this.QueryCapabilities.ReadActive(queryableExtension);
        }

        public async Task<T> ReadActiveById<T>(Guid modelId) where T : IAggregate
        {
            return await this.QueryCapabilities.ReadActiveById<T>(modelId);
        }

        public async Task<Document> ReadById(Guid modelId)
        {
            return await this.QueryCapabilities.ReadById(modelId);
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = true) where T : IAggregate
        {
            return await this.UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public async Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(Guid id, T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            return await this.UpdateCapabilities.UpdateByIdUsingValuesFromAnotherInstance(id, src, overwriteReadOnly);
        }

        public async Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(T src, bool overwriteReadOnly = true)
            where T : IAggregate
        {
            return await this.UpdateCapabilities.UpdateUsingValuesFromAnotherInstanceWithTheSameId(src, overwriteReadOnly);
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : IAggregate
        {
            return await this.UpdateCapabilities.UpdateWhere(predicate, action);
        }
    }
}