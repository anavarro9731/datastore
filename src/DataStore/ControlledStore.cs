namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Options;

    public class ControlledStore : IDataStore
    {
        private readonly ControlFunctions.ControlDataDelegate dataController;

        private readonly DataStore dataStore;

        private readonly IPermission requiredPermissionWithScopeToData;

        private readonly IIdentityWithPermissions user;

        public ControlledStore(
            IIdentityWithPermissions user,
            IPermission requiredPermissionWithScopeToData,
            DataStore dataStore,
            ControlFunctions.ControlDataDelegate dataController)
        {
            if (dataStore.DataStoreOptions.Security == null) throw new Exception("Cannot use security features if they have not enabled via DataStoreOptions");
            this.user = user;
            this.requiredPermissionWithScopeToData = requiredPermissionWithScopeToData;
            this.dataStore = dataStore;
            this.dataController = dataController;
        }

        public IDocumentRepository DocumentRepository => this.dataStore.DocumentRepository;

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations => this.dataStore.ExecutedOperations;

        public IMessageAggregator MessageAggregator => this.dataStore.MessageAggregator;

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations => this.dataStore.QueuedOperations;

        public IWithoutEventReplay WithoutEventReplay => this.dataStore.WithoutEventReplay;

        public IDataStoreQueryCapabilities AsReadOnly()
        {
            return this.dataStore.AsReadOnly();
        }

        public IDataStoreWriteOnlyScoped<T> AsWriteOnlyScoped<T>() where T : class, IAggregate, new()
        {
            return this.dataStore.AsWriteOnlyScoped<T>();
        }

        public Task CommitChanges()
        {
            return this.dataStore.CommitChanges();
        }

        public async Task<T> Create<T>(T model, bool readOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Create(model, readOnly, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> DeleteHard<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class

            var data = await this.dataStore.DeleteHard(instance, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class
            var data = await this.dataStore.DeleteHardById<T>(id, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteHardWhere(predicate, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> DeleteSoft<T>(T instance, string methodName = null) where T : class, IAggregate, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class

            var data = await this.dataStore.DeleteSoft(instance, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class

            var data = await this.dataStore.DeleteSoftById<T>(id, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteSoftWhere(predicate, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Read(predicate, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> Read<T>(string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Read<T>(methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActive(predicate).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> ReadActive<T>(string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActive<T>(methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> ReadActiveById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActiveById<T>(modelId, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> ReadById<T>(Guid modelId, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadById<T>(modelId, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public async Task<T> Update<T, O>(T src, Action<O> setOptions, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class and not here because this is not always top of the callstack

            var data = await this.dataStore.Update(src, setOptions, overwriteReadOnly, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public Task<T> Update<T>(T src, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            return Update<T, UpdateOptions>(src, options => { }, overwriteReadOnly, methodName);
        }

        public async Task<T> UpdateById<T, O>(Guid id, Action<T> action, Action<O> setOptions, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            //- don't delegate calls even though you could since the methodname is set in the DataStore class

            var data = await this.dataStore.UpdateById(id, action, setOptions, overwriteReadOnly, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            return UpdateById<T, UpdateOptions>(id, action, options => { }, overwriteReadOnly, methodName);
        }

        public async Task<IEnumerable<T>> UpdateWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<O> setOptions,
            bool overwriteReadOnly = false,
            string methodName = null) where T : class, IAggregate, new() where O : class, IUpdateOptions, new()
        {
            var data = await this.dataStore.UpdateWhere(predicate, action, setOptions, overwriteReadOnly, methodName).ConfigureAwait(false);

            return data = await ControlData(data).ConfigureAwait(false);
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new()
        {
            return UpdateWhere<T, UpdateOptions>(predicate, action, options => { }, overwriteReadOnly, methodName);
        }

        private async Task<IEnumerable<T>> ControlData<T>(IEnumerable<T> data) where T : class, IAggregate, new()
        {
            var result = await this.dataController(
                             this.user,
                             this.requiredPermissionWithScopeToData,
                             data.Cast<IHaveScope>().ToList(),
                             this.dataStore.DataStoreOptions.Security,
                             this.dataStore).ConfigureAwait(false);

            return result.Cast<T>();
        }

        private async Task<T> ControlData<T>(T data) where T : class, IAggregate, new()
        {
            var result = await ControlData(
                             new[]
                             {
                                 data
                             }).ConfigureAwait(false);

            return result.SingleOrDefault();
        }
    }

    public static class DataStoreExtensions
    {
        public static ControlledStore FilterByPermission(this IDataStore dataStore, IPermission requiredPermissionWithScopeToData, IIdentityWithPermissions user)
        {
            return new ControlledStore(user, requiredPermissionWithScopeToData, (DataStore)dataStore, ControlFunctions.Filter);
        }

        public static ControlledStore RequirePermission(this IDataStore dataStore, IPermission requiredPermissionWithScopeToData, IIdentityWithPermissions user)
        {
            return new ControlledStore(user, requiredPermissionWithScopeToData, (DataStore)dataStore, ControlFunctions.Authorise);
        }
    }
}