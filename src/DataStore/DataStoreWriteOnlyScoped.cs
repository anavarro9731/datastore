namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.LowLevel;
    using MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStoreWriteOnly<T> : IDataStoreWriteOnlyScoped<T> where T : class, IAggregate, new()
    {
        private readonly IMessageAggregator messageAggregator;

        public DataStoreWriteOnly(IDocumentRepository documentRepository, IMessageAggregator messageAggregator = null)
        {
            this.messageAggregator = messageAggregator ?? DataStoreMessageAggregator.Create();
            DsConnection = documentRepository;
            UpdateCapabilities = new DataStoreUpdateCapabilities(DsConnection, messageAggregator);
            DeleteCapabilities = new DataStoreDeleteCapabilities(DsConnection, messageAggregator);
            CreateCapabilities = new DataStoreCreateCapabilities(DsConnection, messageAggregator);
        }

        public IDocumentRepository DsConnection { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        public async Task CommitChanges()
        {
            var dataStoreEvents = messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>();

            foreach (var dataStoreWriteEvent in dataStoreEvents)
                await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
        }

        #region IDataStoreWriteOnlyScoped<T> Members

        public Task<T> Create(T model, bool readOnly = false) 
        {
            return CreateCapabilities.Create(model, readOnly);
        }

        public  Task<IEnumerable<T>> DeleteHardWhere(Expression<Func<T, bool>> predicate)
        {
            return  DeleteCapabilities.DeleteHardWhere(predicate);
        }

        public  Task<T> DeleteSoftById(Guid id)
        {
            return  DeleteCapabilities.DeleteSoftById<T>(id);
        }

        public  Task<T> DeleteHardById(Guid id)
        {
            return  DeleteCapabilities.DeleteHardById<T>(id);
        }


        public  Task<IEnumerable<T>> DeleteSoftWhere(Expression<Func<T, bool>> predicate)
        {
            return  DeleteCapabilities.DeleteSoftWhere(predicate);
        }

        public void Dispose()
        {
            DsConnection.Dispose();
        }

        public  Task<T> UpdateById(Guid id, Action<T> action, bool overwriteReadOnly = true)
        {
            return  UpdateCapabilities.UpdateById(id, action, overwriteReadOnly);
        }

        public  Task<T> Update(T src, bool overwriteReadOnly = true)
        {
            return  UpdateCapabilities.Update(src, overwriteReadOnly);
        }

        public  Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false)
        {
            return  UpdateCapabilities.UpdateWhere(predicate, action);
        }

        #endregion
    }
}