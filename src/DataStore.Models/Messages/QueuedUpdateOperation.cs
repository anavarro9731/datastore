namespace DataStore.Models.Messages
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;

    public class QueuedUpdateOperation<T> : IQueuedDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public QueuedUpdateOperation(string methodCalled, T model, IDocumentRepository repo, IMessageAggregator messageAggregator, Action<string> updateEtag)
        {
            CommitClosure = async () =>
                {
                    await messageAggregator.CollectAndForward(
                        new UpdateOperation<T>
                        {
                            TypeName = typeof(T).FullName,
                            MethodCalled = methodCalled,
                            Created = DateTime.UtcNow,
                            Model = model,
                        }).To(repo.UpdateAsync).ConfigureAwait(false);
                    Committed = true;
                    /* Committed=true has to happen before update eTag is called,
                     there is logic that responds to updateEtag which expects the item causing the update
                     to be committed */
                    updateEtag(model.Etag);
                };

            Created = DateTime.UtcNow;
            Model = model;
            AggregateId = model.id;
        }

        IHaveAUniqueId IQueuedDataStoreWriteOperation.Model => Model;

        public Guid AggregateId { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public DateTime Created { get; set; }

        public T Model { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan StateOperationDuration { get; set; }
    }

    public class UpdateOperation<T> : IDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public DateTime Created { get; set; }

        public string MethodCalled { get; set; }

        public T Model { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }

    }
}