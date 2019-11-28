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

    public class SecureDataStore : IDataStore
    {
        private readonly DataStore dataStore;

        private readonly IPermission requiredPermissionWithScopeToData;

        private readonly IIdentityWithPermissions user;

        public SecureDataStore(IIdentityWithPermissions user, IPermission requiredPermissionWithScopeToData, DataStore dataStore)
        {
            if (dataStore.DataStoreOptions.Security == null) throw new Exception("Cannot use security features if they have not enabled via DataStoreOptions");
            this.user = user;
            this.requiredPermissionWithScopeToData = requiredPermissionWithScopeToData;
            this.dataStore = dataStore;
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
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteHardById<T>(id, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteHardWhere(predicate, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteSoftById<T>(id, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.DeleteSoftWhere(predicate, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public void Dispose()
        {
            this.dataStore.Dispose();
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Read(predicate).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> Read<T>() where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Read<T>().ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActive(predicate).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> ReadActive<T>() where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActive<T>().ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<T> ReadActiveById<T>(Guid modelId) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.ReadActiveById<T>(modelId).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<T> Update<T>(T src, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.Update(src, overwriteReadOnly, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.UpdateById(id, action, overwriteReadOnly, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        public async Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false,
            string methodName = null) where T : class, IAggregate, new()
        {
            var data = await this.dataStore.UpdateWhere(predicate, action, overwriteReadOnly, methodName).ConfigureAwait(false);
            ;
            await AuthoriseData(data).ConfigureAwait(false);
            ;
            return data;
        }

        private async Task AuthoriseData<T>(IEnumerable<T> data) where T : class, IAggregate, new()
        {
            await AuthorisationFunctions.Authorise(
                this.user,
                this.requiredPermissionWithScopeToData,
                data.Cast<IHaveScope>().ToList(),
                this.dataStore.DataStoreOptions.Security,
                this.dataStore).ConfigureAwait(false);
            ;
        }

        private async Task AuthoriseData<T>(T data) where T : class, IAggregate, new()
        {
            await AuthoriseData(
                new[]
                {
                    data
                }).ConfigureAwait(false);
            ;
        }
    }

    public static class DataStoreExtensions
    {
        public static SecureDataStore RequirePermission(this IDataStore dataStore, IPermission requiredPermissionWithScopeToData, IIdentityWithPermissions user)
        {
            return new SecureDataStore(user, requiredPermissionWithScopeToData, (DataStore)dataStore);
        }
    }
}