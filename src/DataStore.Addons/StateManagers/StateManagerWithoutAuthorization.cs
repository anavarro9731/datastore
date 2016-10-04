namespace DataStore.Infrastructure.Impl.StateManagers
{
    using System;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    //using Apogee.Azure.Website;
    //using Apogee.Storage;

    public class StateManagerWithoutAuthorization : IStateManagerWithoutAuthorization
    {
        public StateManagerWithoutAuthorization(IDocumentRepository repository, IEventAggregator eventAggregator)
        {
            DocumentDbPrimary = new DataStore(repository, eventAggregator);
            //FileStoragePrimary = fileStorage;
            TransactionId = Guid.NewGuid();
        }

        public IDataStore DocumentDbPrimary { get; }

        public Guid TransactionId { get; set; }

//        public IFileStorageProvider FileStoragePrimary { get; }

        public virtual void Dispose()
        {
            this.DocumentDbPrimary.Dispose();
        }

        public virtual void SubmitChanges()
        {
            //this.DocumentDbPrimary.CommitChanges();
        }
    }
}