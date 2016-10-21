namespace DataStore
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces.Events;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStoreWriteOnly<T> : IDataStoreWriteOnlyScoped<T> where T : IAggregate, new()
    {
        private readonly IEventAggregator _eventAggregator;

        public DataStoreWriteOnly(IDocumentRepository documentRepository, IEventAggregator eventAggregator = null)
        {
            _eventAggregator = eventAggregator ?? new EventAggregator();
            DsConnection = documentRepository;
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, eventAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, eventAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, eventAggregator);
        }

        public IDocumentRepository DsConnection { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        #region IDataStoreWriteOnlyScoped<T> Members

        public Task<T> Create(T model, bool readOnly = false)
        {
            return CreateCapabilities.Create(model, readOnly);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere(Expression<Func<T, bool>> predicate)
        {
            return await DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public async Task<T> DeleteSoftById(Guid id)
        {
            return await DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public async Task<T> DeleteHardById(Guid id)
        {
            return await DeleteCapabilities.DeleteHardById<T>(id);
        }


        public async Task<IEnumerable<T>> DeleteSoftWhere(Expression<Func<T, bool>> predicate)
        {
            return await DeleteCapabilities.DeleteSoftWhere(predicate);
        }

        public void Dispose()
        {
            DsConnection.Dispose();
        }

        public async Task<T> UpdateById(Guid id, Action<T> action, bool overwriteReadOnly = true)
        {
            return await UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public async Task<T> Update(T src, bool overwriteReadOnly = true)
        {
            return await UpdateCapabilities.Update(src, overwriteReadOnly);
        }

        public async Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false)
        {
            return await UpdateCapabilities.UpdateWhere(predicate, action);
        }

        #endregion

        public async Task CommitChanges()
        {
            var dataStoreEvents = _eventAggregator.Events.OfType<IDataStoreWriteEvent>();

            foreach (var dataStoreWriteEvent in dataStoreEvents)
            {
                await dataStoreWriteEvent.CommitClosure();

            }
        }
    }
}