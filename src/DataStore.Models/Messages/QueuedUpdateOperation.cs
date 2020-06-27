namespace DataStore.Models.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Models.PureFunctions.Extensions;

    public class QueuedUpdateOperation<T> : IQueuedDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public QueuedUpdateOperation(
            string methodCalled,
            T newModel,
            T previousModel,
            IDocumentRepository repo,
            IMessageAggregator messageAggregator,
            Action<string> etagUpdated)
        {
            CommitClosure = async () =>
                {
                await messageAggregator.CollectAndForward(
                    new UpdateOperation<T>
                    {
                        TypeName = typeof(T).FullName,
                        MethodCalled = methodCalled,
                        Created = DateTime.UtcNow,
                        Model = newModel
                    }).To(repo.UpdateAsync).ConfigureAwait(false);
                Committed = true;
                /* Committed=true has to happen before update eTag is called,
                 there is logic that responds to etagUpdated which expects the item causing the update
                 to be committed */
                etagUpdated(newModel.Etag); //* newModel = Model on UpdateOperation which is updated via interface in the repo
                };

            Created = DateTime.UtcNow;
            PreviousModel = previousModel;
            NewModel = newModel;
            AggregateId = newModel.id;
        }

        public Guid AggregateId { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public DateTime Created { get; set; }

        public long? LastModified => (int)PreviousModel.ModifiedAsMillisecondsEpochTime;

        public T NewModel { get; set; }

        public T PreviousModel { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan StateOperationDuration { get; set; }

        IAggregate IQueuedDataStoreWriteOperation.NewModel => NewModel;

        IAggregate IQueuedDataStoreWriteOperation.PreviousModel => PreviousModel;
    }

    public class UpdateOperation<T> : IDataStoreWriteOperation<T> where T : class, IAggregate, new()
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