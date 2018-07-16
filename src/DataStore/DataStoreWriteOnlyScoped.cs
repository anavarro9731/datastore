namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStoreWriteOnly<T> : IDataStoreWriteOnlyScoped<T> where T : class, IAggregate, new()
    {
        private readonly IDataStore dataStore;

        public DataStoreWriteOnly(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task CommitChanges()
        {
            await this.dataStore.CommitChanges();
        }

        public Task<T> Create(T model, bool readOnly = false, string methodName = null)
        {
            return this.dataStore.Create(model, readOnly, methodName);
        }

        public Task<T> DeleteHardById(Guid id, string methodName = null)
        {
            return this.dataStore.DeleteHardById<T>(id, methodName);
        }

        public Task<IEnumerable<T>> DeleteHardWhere(Expression<Func<T, bool>> predicate, string methodName = null)
        {
            return this.dataStore.DeleteHardWhere(predicate, methodName);
        }

        public Task<T> DeleteSoftById(Guid id, string methodName = null)
        {
            return this.dataStore.DeleteSoftById<T>(id, methodName);
        }

        public Task<IEnumerable<T>> DeleteSoftWhere(Expression<Func<T, bool>> predicate, string methodName = null)
        {
            return this.dataStore.DeleteSoftWhere(predicate, methodName);
        }

        public Task<T> Update(T src, bool overwriteReadOnly = true, string methodName = null)
        {
            return this.dataStore.Update(src, overwriteReadOnly, methodName);
        }

        public Task<T> UpdateById(Guid id, Action<T> action, bool overwriteReadOnly = true, string methodName = null)
        {
            return this.dataStore.UpdateById(id, action, overwriteReadOnly, methodName);
        }

        public Task<IEnumerable<T>> UpdateWhere(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false, string methodName = null)
        {
            return this.dataStore.UpdateWhere(predicate, action, overwriteReadOnly, methodName);
        }
    }
}