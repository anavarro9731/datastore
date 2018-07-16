namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;

    // Not sure if eventreplay makes sense in this class, needs review currently its not implemented.
    // It's also questionable what happens to events subsquent to a hard-delete in a session, how does it error?

    internal class DataStoreDeleteCapabilities : IDataStoreDeleteCapabilities
    {
        private readonly IMessageAggregator messageAggregator;

        public DataStoreDeleteCapabilities(IDocumentRepository dataStoreConnection, IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, id, typeof(T)))
                                   .To(DsConnection.GetItemAsync<T>).ConfigureAwait(false);

            if (result == null) return null;

            this.messageAggregator.Collect(new QueuedHardDeleteOperation<T>(methodName, result, DsConnection, this.messageAggregator));

            //clone otherwise its to easy to change the referenced object before committing
            return result.Clone();
        }

        public async Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var objects = await this.messageAggregator
                                    .CollectAndForward(new AggregatesQueriedOperation<T>(methodName, DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                                    .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            var dataObjects = objects as T[] ?? objects.ToArray();

            if (!dataObjects.Any()) return dataObjects;

            foreach (var dataObject in dataObjects)
                this.messageAggregator.Collect(new QueuedHardDeleteOperation<T>(methodName, dataObject, DsConnection, this.messageAggregator));

            //clone otherwise its to easy to change the referenced object before committing
            return dataObjects.Select(d => d.Clone());
        }

        public async Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new()
        {
            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(methodName, id, typeof(T)))
                                   .To(DsConnection.GetItemAsync<T>).ConfigureAwait(false);

            if (result == null) return null;

            MarkAsSoftDeleted(result);

            this.messageAggregator.Collect(new QueuedUpdateOperation<T>(methodName, result, DsConnection, this.messageAggregator));

            //clone otherwise its to easy to change the referenced object before committing
            return result.Clone();
        }

        // .. soft delete one or more DataObjects 
        public async Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new()
        {
            var objects = await this.messageAggregator
                                    .CollectAndForward(new AggregatesQueriedOperation<T>(methodName, DsConnection.CreateDocumentQuery<T>().Where(predicate)))
                                    .To(DsConnection.ExecuteQuery).ConfigureAwait(false);

            var dataObjects = objects as T[] ?? objects.ToArray();

            if (!dataObjects.Any()) return dataObjects;

            foreach (var dataObject in dataObjects)
            {
                MarkAsSoftDeleted(dataObject);
                this.messageAggregator.Collect(new QueuedUpdateOperation<T>(methodName, dataObject, DsConnection, this.messageAggregator));
            }

            //clone otherwise its to easy to change the referenced object before committing
            return dataObjects.Select(o => o.Clone());
        }

        private static void MarkAsSoftDeleted<T>(T dataObject) where T : class, IAggregate, new()
        {
            dataObject.Active = false;
        }
    }
}