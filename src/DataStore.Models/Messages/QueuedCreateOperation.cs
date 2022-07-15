namespace DataStore.Models.Messages
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Models.PureFunctions.Extensions;

    #endregion

    public class QueuedCreateOperation<T> : IQueuedDataStoreWriteOperation<T>, ICanBeUpdatedWhileQueued<T>
        where T : class, IAggregate, new()
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly IDocumentRepository repo;

        public QueuedCreateOperation(
            string methodCalled,
            T model,
            IDocumentRepository repo,
            IMessageAggregator messageAggregator)
        {
            this.repo = repo;
            this.messageAggregator = messageAggregator;
            SetCommitClosure(methodCalled, model, repo, messageAggregator);

            Created = DateTime.UtcNow;
            PreviousModel = null;
            NewModel = model;
            AggregateId = model.id;
        }

        public Guid AggregateId { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public DateTime Created { get; set; }

        public long? LastModified => null;

        public T NewModel { get; set; }

        public T PreviousModel { get; set; }

        IAggregate IQueuedDataStoreWriteOperation.NewModel => NewModel;

        IAggregate IQueuedDataStoreWriteOperation.PreviousModel => PreviousModel;

        public void UpdateModelWhileQueued(string methodCalled, Action<T> update)
        {
            update(NewModel);
            SetCommitClosure(methodCalled, NewModel, this.repo, this.messageAggregator);
        }

        private void SetCommitClosure(
            string methodCalled,
            T model,
            IDocumentRepository repo,
            IMessageAggregator messageAggregator)
        {
            CommitClosure = async () =>
                {
                await messageAggregator.CollectAndForward(
                    new CreateOperation<T>
                    {
                        TypeName = typeof(T).FullName,
                        MethodCalled = methodCalled,
                        Created = DateTime.UtcNow,
                        Model = model
                    }).To(repo.AddAsync).ConfigureAwait(false);
                Committed = true;
                /* Committed=true has to happen before update eTag is called,
                 there is logic that responds to etagUpdated which expects the item causing the update
                 to be committed */
                (model as IEtagUpdated)?.EtagUpdated(model.Etag); //* model = Model on UpdateOperation which is updated via interface in the repo
                };
        }
    }

    public class CreateOperation<T> : IDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public DateTime Created { get; set; }

        public List<Aggregate.AggregateVersionInfo> GetHistoryItems => Model.VersionHistory;

        public string MethodCalled { get; set; }

        public T Model { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }

        IAggregate IDataStoreWriteOperation.Model { set => Model = value.As<T>(); }
    }
}