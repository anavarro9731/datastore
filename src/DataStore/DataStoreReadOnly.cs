namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Interfaces.Options.ClientSide;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    public class DataStoreReadOnly : IDataStoreReadOnly
    {
        private readonly DataStore dataStore;
        
        public DataStoreReadOnly(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }
        
        public IWithoutEventReplay WithoutEventReplay =>
            new WithoutEventReplay(this.dataStore.DocumentRepository, this.dataStore.MessageAggregator, this.dataStore.ControlFunctions ,this.dataStore.DataStoreOptions);

        public Task<IEnumerable<T>> Read<T, O>(
            Expression<Func<T, bool>> predicate = null,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : ReadClientSideBaseOptions, new() =>
            this.dataStore.Read(predicate, setOptions, methodName);

        public Task<IEnumerable<T>> Read<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<ReadClientSideOptions> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() =>
            this.dataStore.Read(predicate, setOptions, methodName);

        public Task<IEnumerable<T>> ReadActive<T, O>(
            Expression<Func<T, bool>> predicate = null,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : ReadClientSideBaseOptions, new() =>
            this.dataStore.ReadActive(predicate, setOptions, methodName);

        public Task<IEnumerable<T>> ReadActive<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<ReadClientSideOptions> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() =>
            this.dataStore.ReadActive(predicate, setOptions, methodName);

        public Task<T> ReadActiveById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadClientSideBaseOptions, new() =>
            this.dataStore.ReadActiveById<T, O>(modelId, setOptions, methodName);

        public Task<T> ReadActiveById<T>(Guid modelId, Action<ReadClientSideOptions> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() =>
            this.dataStore.ReadActiveById<T>(modelId, setOptions, methodName);

        public Task<T> ReadActiveById<T>(string longId, Action<ReadClientSideBaseOptions> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return ReadActiveById<T>(
                keys.Id,
                SetLongIdOptions(setOptions, keys), methodName);
        }

        public Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadClientSideBaseOptions, new() =>
            this.dataStore.ReadById<T, O>(modelId, setOptions, methodName);

        public Task<T> ReadById<T>(Guid modelId, Action<ReadClientSideOptions> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() =>
            this.dataStore.ReadById<T>(modelId, setOptions, methodName);

        public Task<T> ReadById<T>(string longId, Action<ReadClientSideBaseOptions> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            var keys = PartitionKeyHelpers.DestructurePartitionedIdString(longId);
            return ReadById<T>(
                keys.Id,
                SetLongIdOptions(setOptions, keys), methodName);
        }

        internal static Action<ReadClientSideOptions> SetLongIdOptions(Action<ReadClientSideBaseOptions> setOptions, Aggregate.PartitionedId keys)
        {
            return o =>
                {
                setOptions?.Invoke(o);
                if (keys.TenantId.HasValue && keys.TimePeriod != default)
                    o.ProvidePartitionKeyValues(keys.TenantId.Value, keys.TimePeriod);
                else if (keys.TimePeriod != default)
                {
                    o.ProvidePartitionKeyValues(keys.TimePeriod);
                } else if (keys.TenantId.HasValue)
                {
                    o.ProvidePartitionKeyValues(keys.TenantId.Value);
                }
                };
        }
    }
}