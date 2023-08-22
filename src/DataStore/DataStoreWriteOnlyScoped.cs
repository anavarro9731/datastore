namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options.ClientSide;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    /// <summary>
    ///     Limits writes to a single aggregate type
    /// </summary>
    public class DataStoreWriteOnly<T> : IDataStoreWriteOnly<T> where T : class, IAggregate, new()
    {
        private readonly DataStore dataStore;

        public DataStoreWriteOnly(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations => this.dataStore.ExecutedOperations;

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations => this.dataStore.QueuedOperations;

        public Task CommitChanges()
        {
            return this.dataStore.CommitChanges();
        }

        public Task<T> Create<O>(T model, Action<O> setOptions = null, string methodName = null) where O : CreateClientSideOptions, new()
        {
            return this.dataStore.Create(model, setOptions, methodName);
        }

        public Task<T> Create(T model, Action<CreateClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.Create(model, setOptions, methodName);
        }

        public Task<T> Delete<O>(T instance, Action<O> setOptions = null, string methodName = null) where O : DeleteClientSideOptions, new()
        {
            return this.dataStore.Delete(instance, setOptions, methodName);
        }

        public Task<T> Delete(T instance, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.Delete(instance, setOptions, methodName);
        }

        public Task<T> DeleteById<O>(Guid id, Action<O> setOptions = null, string methodName = null) where O : DeleteClientSideOptions, new()
        {
            return this.dataStore.DeleteById<T, O>(id, setOptions, methodName);
        }

        public Task<T> DeleteById(Guid id, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.DeleteById<T>(id, setOptions, methodName);
        }

        public Task<T> DeleteById(string longId, Action<DeleteClientSideOptions> setOptions = null, string methodName = null) 
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return DeleteById(keys.Id, SetLongIdDeleteOptions(setOptions, keys), methodName);
        }

        public Task<IEnumerable<T>> DeleteWhere<O>(Expression<Func<T, bool>> predicate, Action<O> setOptions = null, string methodName = null)
            where O : DeleteClientSideOptions, new()
        {
            return this.dataStore.DeleteWhere(predicate, setOptions, methodName);
        }

        public Task<IEnumerable<T>> DeleteWhere(Expression<Func<T, bool>> predicate, Action<DeleteClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.DeleteWhere(predicate, setOptions, methodName);
        }

        public Task<T> Update<O>(T src, Action<O> setOptions = null, string methodName = null) where O : UpdateClientSideOptions, new()
        {
            return this.dataStore.Update(src, setOptions, methodName);
        }

        public Task<T> Update(T src, Action<UpdateClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.Update(src, setOptions, methodName);
        }

        public Task<T> UpdateById<O>(Guid id, Action<T> action, Action<O> setOptions = null, string methodName = null) where O : UpdateClientSideOptions, new()
        {
            return this.dataStore.UpdateById(id, action, setOptions, methodName);
        }

        public Task<T> UpdateById(Guid id, Action<T> action, Action<UpdateClientSideOptions> setOptions = null, string methodName = null)
        {
            return this.dataStore.UpdateById(id, action, setOptions, methodName);
        }

        public Task<T> UpdateById(string longId, Action<T> action, Action<UpdateClientSideOptions> setOptions = null, string methodName = null)
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return UpdateById(keys.Id, action, SetLongIdUpdateOptions(setOptions, keys), methodName);
        }

        public Task<IEnumerable<T>> UpdateWhere<O>(Expression<Func<T, bool>> predicate, Action<T> action, Action<O> setOptions = null, string methodName = null)
            where O : UpdateClientSideOptions, new()
        {
            return this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);
        }

        public Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<UpdateClientSideOptions> setOptions = null,
            string methodName = null)
        {
            return this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);
        }

        internal static Action<UpdateClientSideOptions> SetLongIdUpdateOptions(Action<UpdateClientSideOptions> setOptions, Aggregate.PartitionedId keys)
        {
            return o =>
                {
                setOptions?.Invoke(o);
                if (keys.TenantId.HasValue && keys.TimePeriod != default)
                {
                    o.ProvidePartitionKeyValues(keys.TenantId.Value, keys.TimePeriod);
                }
                else if (keys.TimePeriod != default)
                {
                    o.ProvidePartitionKeyValues(keys.TimePeriod);
                }
                else if (keys.TenantId.HasValue)
                {
                    o.ProvidePartitionKeyValues(keys.TenantId.Value);
                }
                };
        }

        internal static Action<DeleteClientSideOptions> SetLongIdDeleteOptions(Action<DeleteClientSideOptions> setOptions, Aggregate.PartitionedId keys)
        {
            return o =>
                {
                setOptions?.Invoke(o);
                if (keys.TenantId.HasValue && keys.TimePeriod != default)
                {
                    o.ProvidePartitionKeyValues(keys.TenantId.Value, keys.TimePeriod);
                }
                else if (keys.TimePeriod != default)
                {
                    o.ProvidePartitionKeyValues(keys.TimePeriod);
                }
                else if (keys.TenantId.HasValue)
                {
                    o.ProvidePartitionKeyValues(keys.TenantId.Value);
                }
                };
        }
    }
}