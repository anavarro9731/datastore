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

        public Task<T> Create<O>(T model, Action<O> setOptions = null, string methodName = null) where O : CreateOptionsClientSideBase, new()
        {
            return this.dataStore.Create(model, setOptions, methodName);
        }

        public Task<T> Create(T model, Action<CreateOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.Create(model, setOptions, methodName);
        }

        public Task<T> Delete<O>(T instance, Action<O> setOptions = null, string methodName = null) where O : DeleteOptionsClientSideBase, new()
        {
            return this.dataStore.Delete(instance, setOptions, methodName);
        }

        public Task<T> Delete(T instance, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.Delete(instance, setOptions, methodName);
        }

        public Task<T> DeleteById<O>(Guid id, Action<O> setOptions = null, string methodName = null) where O : DeleteOptionsClientSideBase, new()
        {
            return this.dataStore.DeleteById<T, O>(id, setOptions, methodName);
        }

        public Task<T> DeleteById(Guid id, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.DeleteById<T>(id, setOptions, methodName);
        }

        public Task<T> DeleteById(string longId, Action<DeleteOptionsClientSideBase> setOptions = null, string methodName = null) 
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return DeleteById<DeleteOptionsClientSide>(keys.Id, SetLongIdDeleteOptions(setOptions, keys));
        }

        public Task<IEnumerable<T>> DeleteWhere<O>(Expression<Func<T, bool>> predicate, Action<O> setOptions = null, string methodName = null)
            where O : DeleteOptionsClientSideBase, new()
        {
            return this.dataStore.DeleteWhere(predicate, setOptions, methodName);
        }

        public Task<IEnumerable<T>> DeleteWhere(Expression<Func<T, bool>> predicate, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.DeleteWhere(predicate, setOptions, methodName);
        }

        public Task<T> Update<O>(T src, Action<O> setOptions = null, string methodName = null) where O : UpdateOptionsClientSideBase, new()
        {
            return this.dataStore.Update(src, setOptions, methodName);
        }

        public Task<T> Update(T src, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.Update(src, setOptions, methodName);
        }

        public Task<T> UpdateById<O>(Guid id, Action<T> action, Action<O> setOptions = null, string methodName = null) where O : UpdateOptionsClientSideBase, new()
        {
            return this.dataStore.UpdateById(id, action, setOptions, methodName);
        }

        public Task<T> UpdateById(Guid id, Action<T> action, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
        {
            return this.dataStore.UpdateById(id, action, setOptions, methodName);
        }

        public Task<T> UpdateById(string longId, Action<T> action, Action<UpdateOptionsClientSideBase> setOptions = null, string methodName = null)
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return UpdateById<UpdateOptionsClientSide>(keys.Id, action, SetLongIdUpdateOptions(setOptions, keys));
        }

        public Task<IEnumerable<T>> UpdateWhere<O>(Expression<Func<T, bool>> predicate, Action<T> action, Action<O> setOptions = null, string methodName = null)
            where O : UpdateOptionsClientSideBase, new()
        {
            return this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);
        }

        public Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<UpdateOptionsClientSide> setOptions = null,
            string methodName = null)
        {
            return this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);
        }

        private static Action<UpdateOptionsClientSide> SetLongIdUpdateOptions(Action<UpdateOptionsClientSideBase> setOptions, Aggregate.PartitionedId keys)
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
        
        private static Action<DeleteOptionsClientSide> SetLongIdDeleteOptions(Action<DeleteOptionsClientSideBase> setOptions, Aggregate.PartitionedId keys)
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