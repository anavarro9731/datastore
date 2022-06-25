namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;

    /// <summary>
    ///     Limits writes to a single aggregate type
    /// </summary>
    public class DataStoreWriteOnly<T> : IDataStoreWriteOnly where T : class, IAggregate, new()
    {
        private readonly DataStore dataStore;

        public DataStoreWriteOnly(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations => this.dataStore.ExecutedOperations;

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations => this.dataStore.QueuedOperations;

        public Task CommitChanges() => this.dataStore.CommitChanges();

        public Task<T1> Create<T1, O>(T1 model, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : CreateOptionsClientSide, new() =>
            this.dataStore.Create(model, setOptions, methodName);

        public Task<T1> Create<T1>(T1 model, Action<CreateOptionsClientSide> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() =>
            this.dataStore.Create(model, setOptions, methodName);

        public Task<T1> Delete<T1, O>(T1 instance, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : DeleteOptionsClientSide, new() =>
            this.dataStore.Delete(instance, setOptions, methodName);

        public Task<T1> Delete<T1>(T1 instance, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() =>
            this.dataStore.Delete(instance, setOptions, methodName);

        public Task<T1> DeleteById<T1, O>(Guid id, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : DeleteOptionsClientSide, new() =>
            this.dataStore.DeleteById<T1, O>(id, setOptions, methodName);

        public Task<T1> DeleteById<T1>(Guid id, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() =>
            this.dataStore.DeleteById<T1>(id, setOptions, methodName);

        public Task<IEnumerable<T1>> DeleteWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : DeleteOptionsClientSide, new() =>
            this.dataStore.DeleteWhere(predicate, setOptions, methodName);

        public Task<IEnumerable<T1>> DeleteWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<DeleteOptionsClientSide> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() =>
            this.dataStore.DeleteWhere(predicate, setOptions, methodName);

        public Task<T1> Update<T1, O>(T1 src, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : UpdateOptionsClientSide, new() =>
            this.dataStore.Update(src, setOptions, methodName);

        public Task<T1> Update<T1>(T1 src, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() =>
            this.dataStore.Update(src, setOptions, methodName);

        public Task<T1> UpdateById<T1, O>(Guid id, Action<T1> action, Action<O> setOptions = null, string methodName = null)
            where T1 : class, IAggregate, new() where O : UpdateOptionsClientSide, new() =>
            this.dataStore.UpdateById(id, action, setOptions, methodName);

        public Task<T1> UpdateById<T1>(
            Guid id,
            Action<T1> action,
            Action<UpdateOptionsClientSide> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() =>
            this.dataStore.UpdateById(id, action, setOptions, methodName);

        public Task<IEnumerable<T1>> UpdateWhere<T1, O>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<O> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() where O : UpdateOptionsClientSide, new() =>
            this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);

        public Task<IEnumerable<T1>> UpdateWhere<T1>(
            Expression<Func<T1, bool>> predicate,
            Action<T1> action,
            Action<UpdateOptionsClientSide> setOptions = null,
            string methodName = null) where T1 : class, IAggregate, new() =>
            this.dataStore.UpdateWhere(predicate, action, setOptions, methodName);
    }
}