namespace DataStore.Models.Messages
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;

    public class QueuedHardDeleteOperation<T> : IQueuedDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public QueuedHardDeleteOperation(string methodCalled, T model, IDocumentRepository repo, IMessageAggregator messageAggregator)
        {
            CommitClosure = async () =>
                {
                await messageAggregator.CollectAndForward(
                    new HardDeleteOperation<T>
                    {
                        TypeName = typeof(T).FullName,
                        MethodCalled = methodCalled,
                        Created = DateTime.UtcNow,
                        Model = model
                    }).To(repo.DeleteHardAsync).ConfigureAwait(false);

                Committed = true;
                };

            Created = DateTime.UtcNow;
            Model = model;
            AggregateId = model.id;
        }

        public Guid AggregateId { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public DateTime Created { get; set; }

        public T Model { get; set; }
    }

    public class HardDeleteOperation<T> : IDataStoreWriteOperation<T> where T : class, IAggregate, new()
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