namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    /// <summary>
    ///     Facade over querying and unit of work capabilities
    ///     Derived non-generic shorthand when a single or primary store exists
    /// </summary>
    public class DataStore : IDataStore
    {
        private readonly DataStoreOptions dataStoreOptions;

        public DataStore(IDocumentRepository documentRepository, IMessageAggregator eventAggregator = null, DataStoreOptions dataStoreOptions = null)
        {
            {
                ValidateOptions(dataStoreOptions);
                {
                    MessageAggregator = eventAggregator ?? DataStoreMessageAggregator.Create();
                    this.dataStoreOptions = dataStoreOptions ?? Options.DataStoreOptions.Create();

                    DocumentRepository = documentRepository;

                    var incrementVersions = new IncrementVersions(this);

                    QueryCapabilities = new DataStoreQueryCapabilities(DocumentRepository, MessageAggregator, DataStoreOptions);
                    UpdateCapabilities = new DataStoreUpdateCapabilities(DocumentRepository, MessageAggregator, DataStoreOptions, incrementVersions);
                    DeleteCapabilities = new DataStoreDeleteCapabilities(
                        DocumentRepository,
                        UpdateCapabilities,
                        MessageAggregator,
                        DataStoreOptions,
                        incrementVersions);
                    CreateCapabilities = new DataStoreCreateCapabilities(DocumentRepository, MessageAggregator, DataStoreOptions, incrementVersions);

                    ControlFunctions = new ControlFunctions(this);
                }
            }

            void ValidateOptions(DataStoreOptions options)
            {
                //not sure how to handle disabling version history when its already been enabled??
            }
        }

        public IDataStoreOptions DataStoreOptions => this.dataStoreOptions;

        public IDocumentRepository DocumentRepository { get; }

        public IReadOnlyList<IDataStoreOperation> ExecutedOperations => MessageAggregator.AllMessages.OfType<IDataStoreOperation>().ToList().AsReadOnly();

        public IMessageAggregator MessageAggregator { get; }

        public IReadOnlyList<IQueuedDataStoreWriteOperation> QueuedOperations
        {
            get
            {
                var queued = MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().Where(o => o.Committed == false).ToList().AsReadOnly();

                return queued;
            }
        }

        public IWithoutEventReplay WithoutEventReplay => new WithoutEventReplay(DocumentRepository, MessageAggregator, ControlFunctions, DataStoreOptions);

        internal ControlFunctions ControlFunctions { get; }

        private DataStoreCreateCapabilities CreateCapabilities { get; }

        private DataStoreDeleteCapabilities DeleteCapabilities { get; }

        private DataStoreQueryCapabilities QueryCapabilities { get; }

        private DataStoreUpdateCapabilities UpdateCapabilities { get; }

        public IDataStoreReadOnly AsReadOnly()
        {
            return new DataStoreReadOnly(this);
        }

        public IDataStoreWriteOnly AsWriteOnlyScoped<T>() where T : class, IAggregate, new()
        {
            return new DataStoreWriteOnly<T>(this);
        }

        public async Task CommitChanges()
        {
            {
                FilterEvents(out var committableEvents, out var committedEvents);

                await CommitAllEvents(committableEvents).ConfigureAwait(false);
            }

            async Task CommitAllEvents(List<IQueuedDataStoreWriteOperation> committableEvents)
            {
                foreach (var dataStoreWriteEvent in committableEvents)
                    await dataStoreWriteEvent.CommitClosure().ConfigureAwait(false);
            }

            void FilterEvents(out List<IQueuedDataStoreWriteOperation> committableEvents, out List<IQueuedDataStoreWriteOperation> committedEvents)
            {
                var dsEvents = MessageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation>().ToList();
                committableEvents = dsEvents.Where(e => !e.Committed).ToList();
                committedEvents = dsEvents.Where(e => e.Committed).ToList();
            }
        }

        /* the Operation<T,O> makes it possible to have a db specific impl
         while Operation<T> allows you to use the built-in globally shared features only */

        //* Create
        public async Task<T> Create<T, O>(T model, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : CreateOptionsClientSide, new()
        {
            CreateOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(Create));

            var result = await CreateCapabilities.Create(model, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.CREATE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                               .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> Create<T>(T model, Action<CreateOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return Create<T, DefaultCreateOptions>(model, setOptions, methodName);
        }

        //* Delete
        public async Task<T> Delete<T, O>(T instance, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new()
        {
            DeleteOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(Delete));

            var result = await DeleteCapabilities.Delete(instance, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.DELETE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                               .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> Delete<T>(T instance, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return Delete<T, DefaultDeleteOptions>(instance, setOptions, methodName);
        }

        //* DeleteById
        public async Task<T> DeleteById<T, O>(Guid id, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new()
        {
            DeleteOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(DeleteById));

            var result = await DeleteCapabilities.DeleteById<T, DeleteOptionsLibrarySide>(id, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.DELETE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                               .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> DeleteById<T>(Guid id, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return DeleteById<T, DefaultDeleteOptions>(id, setOptions, methodName);
        }

        //* DeleteWhere
        public async Task<IEnumerable<T>> DeleteWhere<T, O>(Expression<Func<T, bool>> predicate, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : DeleteOptionsClientSide, new()
        {
            DeleteOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(DeleteWhere));

            var results = await DeleteCapabilities.DeleteWhere(predicate, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                results = await ControlFunctions.AuthoriseData(results, SecurableOperations.DELETE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                .ConfigureAwait(false);
            }

            return results;
        }

        public Task<IEnumerable<T>> DeleteWhere<T>(Expression<Func<T, bool>> predicate, Action<DeleteOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new()
        {
            return DeleteWhere<T, DefaultDeleteOptions>(predicate, setOptions, methodName);
        }

        //* Read
        public async Task<IEnumerable<T>> Read<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new()
        {
            ReadOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(Read));

            var result = await QueryCapabilities.Read(predicate, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                if (HasPii<T>())
                {
                    result = await ControlFunctions.AuthoriseData(result, SecurableOperations.READPII, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
                else
                {
                    result = await ControlFunctions.AuthoriseData(result, SecurableOperations.READ, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
            }

            return result;
        }

        public Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Action<ReadOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new()
        {
            return Read<T, DefaultReadOptions>(predicate, setOptions, methodName);
        }

        //*ReadActive
        public async Task<IEnumerable<T>> ReadActive<T, O>(Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new()
        {
            ReadOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(ReadActive));

            var result = await QueryCapabilities.ReadActive(predicate, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                if (HasPii<T>())
                {
                    result = await ControlFunctions.AuthoriseData(result, SecurableOperations.READPII, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
                else
                {
                    result = await ControlFunctions.AuthoriseData(result, SecurableOperations.READ, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
            }

            return result;
        }

        public Task<IEnumerable<T>> ReadActive<T>(
            Expression<Func<T, bool>> predicate = null,
            Action<ReadOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new()
        {
            return ReadActive<T, DefaultReadOptions>(predicate, setOptions, methodName);
        }

        //ReadActiveById
        public async Task<T> ReadActiveById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new()
        {
            ReadOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(ReadActiveById));

            var result = await QueryCapabilities.ReadActiveById<T, ReadOptionsLibrarySide>(modelId, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                if (HasPii<T>())
                {
                    result = await ControlFunctions.AuthoriseDatum(
                                 result,
                                 SecurableOperations.READPII,
                                 options.Identity ?? this.dataStoreOptions.Security.SecuredFor).ConfigureAwait(false);
                }
                else
                {
                    result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.READ, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
            }

            return result;
        }

        public Task<T> ReadActiveById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return ReadActiveById<T, DefaultReadOptions>(modelId, setOptions, methodName);
        }

        //ReadById
        public async Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : ReadOptionsClientSide, new()
        {
            ReadOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(ReadById));

            var result = await QueryCapabilities.ReadById<T, ReadOptionsLibrarySide>(modelId, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                if (HasPii<T>())
                {
                    result = await ControlFunctions.AuthoriseDatum(
                                 result,
                                 SecurableOperations.READPII,
                                 options.Identity ?? this.dataStoreOptions.Security.SecuredFor).ConfigureAwait(false);
                }
                else
                {
                    result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.READ, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                   .ConfigureAwait(false);
                }
            }

            return result;
        }

        private static bool HasPii<T>() where T : class, IAggregate, new()
        {
            var hasPii = typeof(T).GetProperties().Any(x => x.GetCustomAttribute(typeof(PIIAttribute), false) != null);
            return hasPii;
        }

        public Task<T> ReadById<T>(Guid modelId, Action<ReadOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return ReadById<T, DefaultReadOptions>(modelId, setOptions, methodName);
        }

        //* Update
        public async Task<T> Update<T, O>(T src, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new()
        {
            UpdateOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(Update));

            var result = await UpdateCapabilities.Update(src, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.UPDATE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                               .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> Update<T>(T src, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null) where T : class, IAggregate, new()
        {
            return Update<T, DefaultUpdateOptions>(src, setOptions, methodName);
        }

        //* UpdateById
        public async Task<T> UpdateById<T, O>(Guid id, Action<T> action, Action<O> setOptions = null, string methodName = null)
            where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new()
        {
            UpdateOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(UpdateById));

            var result = await UpdateCapabilities.UpdateById(id, action, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                result = await ControlFunctions.AuthoriseDatum(result, SecurableOperations.UPDATE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                               .ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> UpdateById<T>(Guid id, Action<T> action, Action<UpdateOptionsClientSide> setOptions = null, string methodName = null)
            where T : class, IAggregate, new()
        {
            return UpdateById<T, DefaultUpdateOptions>(id, action, setOptions, methodName);
        }

        //* UpdateWhere
        public async Task<IEnumerable<T>> UpdateWhere<T, O>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<O> setOptions = null,
            string methodName = null) where T : class, IAggregate, new() where O : UpdateOptionsClientSide, new()
        {
            UpdateOptionsLibrarySide options = setOptions == null ? new O() : new O().Op(setOptions);

            AppendMethodName(ref methodName, nameof(UpdateWhere));

            var results = await UpdateCapabilities.UpdateWhere(predicate, action, options, methodName).ConfigureAwait(false);

            if (SecurityShouldBeApplied<T>(options))
            {
                results = await ControlFunctions.AuthoriseData(results, SecurableOperations.UPDATE, options.Identity ?? this.dataStoreOptions.Security.SecuredFor)
                                                .ConfigureAwait(false);
            }

            return results;
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            Action<UpdateOptionsClientSide> setOptions = null,
            string methodName = null) where T : class, IAggregate, new()
        {
            return UpdateWhere<T, DefaultUpdateOptions>(predicate, action, setOptions, methodName);
        }

        private static void AppendMethodName(ref string current, string add)
        {
            if (string.IsNullOrWhiteSpace(current))
            {
                current += add;
            }
            else
            {
                current += "+" + add;
            }
        }

        private bool SecurityShouldBeApplied<T>(ISecurityOptions options) where T : class, IAggregate, new()
        {
            var applySecurity = this.dataStoreOptions.Security != null && (options.Identity != null || this.dataStoreOptions.Security.SecuredFor != null);
            var bypassSecurityEnabledForThisAggregate = typeof(T).GetCustomAttributes(false).ToList().Exists(x => x.GetType() == typeof(BypassSecurity));
            var bypassSecurityEnabledForThisCall = options.BypassSecurity;
            return applySecurity && !bypassSecurityEnabledForThisAggregate && !bypassSecurityEnabledForThisCall;
        }
    }
}