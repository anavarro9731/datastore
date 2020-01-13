namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;

    internal class Session
    {
        private readonly DataStore dataStore;

        public List<DataStoreUnitOfWork> DataStoreCreateOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreDeleteOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreUpdateOperations { get; internal set; }

        public class DataStoreUnitOfWork : SerialisableObject
        {
            private readonly IDataStore dataStore;

            //- cache for perf
            private bool? isComplete = false;

            public DataStoreUnitOfWork(IHaveAUniqueId x, IDataStore dataStore)
                : base(x)
            {
                this.dataStore = dataStore;
                ObjectId = x.id;
            }

            public Guid ObjectId { get; internal set; }

            public async Task<bool> IsComplete()
            {
                return (bool)(this.isComplete = (this.isComplete ?? await QueryForCompleteness()));
            }

            /* there is a possibility of a race condition here, however as the change history is audited I think we can ignore and
             take compensating action should that ever occur*/
            private async Task<bool> QueryForCompleteness()
            {
                var history = (await this.dataStore.Read<AggregateHistory>(x => x.AggregateId == ObjectId)).SingleOrDefault();
                return history != null;
            }
        }
    }
}